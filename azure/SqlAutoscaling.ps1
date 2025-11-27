param(
    [string] $ResourceGroup = (Get-AutomationVariable -Name 'Autoscale_ResourceGroup'),
    [string] $ServerName = (Get-AutomationVariable -Name 'Autoscale_SqlServerName'),
    [string] $DbName = (Get-AutomationVariable -Name 'Autoscale_DbName'),

    # Service Bus
    [string] $SbResourceGroup = (Get-AutomationVariable -Name 'Autoscale_ResourceGroup'),
    [string] $SbNamespace = (Get-AutomationVariable -Name 'Autoscale_SbNamespace'),
    [string] $SbQueue = (Get-AutomationVariable -Name 'Autoscale_SbQueue'),

    # thresholds and durations
    [int] $ScaleUpThreshold = (Get-AutomationVariable -Name 'Autoscale_ScaleUpThreshold'),   # active messages above -> consider scale up
    [int] $ScaleDownThreshold = (Get-AutomationVariable -Name 'Autoscale_ScaleDownThreshold'),  # active messages below -> consider scale down
    [int] $SustainedUpMinutes = (Get-AutomationVariable -Name 'Autoscale_SustainedUpMinutes'),    # sustained minutes above upper threshold
    [int] $SustainedDownMinutes = (Get-AutomationVariable -Name 'Autoscale_SustainedDownMinutes'), # sustained minutes below lower threshold
    [int] $CooldownMinutes = (Get-AutomationVariable -Name 'Autoscale_CooldownMinutes'),      # don't re-scale within this cooldown window

    # target service objectives
    [string] $ScaleUpTarget = (Get-AutomationVariable -Name 'Autoscale_ScaleUpTarget'),
    [string] $ScaleDownTarget = (Get-AutomationVariable -Name 'Autoscale_ScaleDownTarget'),

    [string] $AutomationVariableName = "SqlAutoScaleLastActionUtc"
)

function Log($msg) {
    Write-Output ("[{0}] {1}" -f (Get-Date -Format o), $msg)
}

function Get-LastScaleTime {
    param(
        [string] $VariableName
    )

    if ([string]::IsNullOrWhiteSpace($VariableName)) {
        return $null
    }

    try {
        if (Get-Command -Name Get-AutomationVariable -ErrorAction SilentlyContinue) {
            $value = Get-AutomationVariable -Name $VariableName -ErrorAction Stop
            if ($null -ne $value -and $value -ne "") {
                # Handle case where value is an array - take the first element
                if ($value -is [Array]) {
                    if ($value.Length -gt 0) {
                        $value = $value[0]
                        Log "Automation variable '$VariableName' returned an array with $($value.Length) element(s). Using first element."
                    } else {
                        Log "Automation variable '$VariableName' returned an empty array. Treating as no value."
                        return $null
                    }
                }
                
                # Handle case where value might already be a DateTime
                if ($value -is [DateTime]) {
                    return $value.ToUniversalTime()
                }
                
                # Ensure value is a string and parse it
                $stringValue = [string]$value
                if (-not [string]::IsNullOrWhiteSpace($stringValue)) {
                    try {
                        $parsedDate = [datetime]::Parse($stringValue, $null, [System.Globalization.DateTimeStyles]::RoundtripKind)
                        if ($parsedDate -is [DateTime]) {
                            return $parsedDate.ToUniversalTime()
                        }
                    }
                    catch {
                        Log "Failed to parse datetime string '$stringValue' from automation variable '$VariableName': $_"
                    }
                }
            }
        }
    }
    catch {
        Log "Automation variable '$VariableName' not found or unreadable: $_"
    }

    return $null
}

function Set-LastScaleTime {
    param(
        [string] $VariableName,
        [datetime] $TimestampUtc
    )

    if ([string]::IsNullOrWhiteSpace($VariableName) -or $null -eq $TimestampUtc) {
        return
    }

    try {
        if (Get-Command -Name Set-AutomationVariable -ErrorAction SilentlyContinue) {
            Set-AutomationVariable -Name $VariableName -Value ($TimestampUtc.ToString("o"))
            Log "Updated automation variable '$VariableName' with timestamp $TimestampUtc"
        }
    }
    catch {
        Log "Unable to update automation variable '$VariableName': $_"
    }
}

function Test-SustainedMetric {
    param(
        [string] $ResourceId,
        [string] $MetricName,
        [int] $DurationMinutes,
        [double] $Threshold,
        [ValidateSet("GreaterOrEqual", "LessOrEqual")]
        [string] $Comparison
    )

    if ($DurationMinutes -le 0) {
        return $true
    }

    $endTime = Get-Date
    $startTime = $endTime.AddMinutes(-1 * $DurationMinutes)
    $timespan = [timespan]::FromMinutes(1)

    try {
        $metric = Get-AzMetric -ResourceId $ResourceId -MetricName $MetricName -TimeGrain $timespan -StartTime $startTime -EndTime $endTime -Aggregation Average -ErrorAction Stop -WarningAction SilentlyContinue
        $datapoints = $metric.Data | Where-Object { $null -ne $_.Average }

        if (-not $datapoints) {
            Log "Metric $MetricName returned no datapoints for $DurationMinutes minutes."
            return $false
        }

        switch ($Comparison) {
            "GreaterOrEqual" {
                $result = ($datapoints | Where-Object { $_.Average -lt $Threshold }).Count -eq 0
            }
            "LessOrEqual" {
                $result = ($datapoints | Where-Object { $_.Average -gt $Threshold }).Count -eq 0
            }
        }

        Log ("Sustained metric check ({0} {1} for {2}m): {3}" -f $MetricName, $Comparison, $DurationMinutes, $result)
        return $result
    }
    catch {
        Log ("Failed to query metric {0} for resource {1}: {2}" -f $MetricName, $ResourceId, $_)
        return $false
    }
}

function Invoke-Scale {
    param(
        [string] $TargetObjective,
        [string] $ResourceGroup,
        [string] $ServerName,
        [string] $DbName
    )

    Log "Requesting new service objective '$TargetObjective' for database '$DbName'..."
    Set-AzSqlDatabase -ResourceGroupName $ResourceGroup -ServerName $ServerName -DatabaseName $DbName -RequestedServiceObjectiveName $TargetObjective -ErrorAction Stop | Out-Null
    Log "Scale operation submitted."
}

Log "Authenticating with managed identity..."
try {
    Connect-AzAccount -Identity -ErrorAction Stop | Out-Null
    Log "Successfully authenticated using managed identity."
}
catch {
    Log "ERROR: Failed to authenticate with managed identity: $_"
    throw "Authentication failed. Managed identity authentication is required."
}

Log "Reading Service Bus queue runtime properties..."
try {
    $queue = Get-AzServiceBusQueue -ResourceGroupName $SbResourceGroup -NamespaceName $SbNamespace -Name $SbQueue -ErrorAction Stop
    $active = [int]$queue.CountDetails.ActiveMessageCount
    Log "Active messages (current snapshot): $active"
}
catch {
    Log "ERROR: Unable to read Service Bus queue: $_"
    throw
}

Log "Reading current database service objective..."
$db = Get-AzSqlDatabase -ResourceGroupName $ResourceGroup -ServerName $ServerName -DatabaseName $DbName -ErrorAction Stop
$currentObjective = $db.CurrentServiceObjectiveName
Log "Current service objective: $currentObjective"

$lastScale = Get-LastScaleTime -VariableName $AutomationVariableName
if ($null -ne $lastScale -and $lastScale -is [DateTime]) {
    Log "Last scale action recorded at (UTC): $lastScale"
    if ($lastScale.AddMinutes($CooldownMinutes) -gt (Get-Date).ToUniversalTime()) {
        Log "Cooldown window still active. No scaling action will be taken."
        return
    }
} else {
    if ($null -ne $lastScale) {
        Log "WARNING: Last scale time is not a valid DateTime object. Type: $($lastScale.GetType().Name). Ignoring cooldown check."
    } else {
        Log "No last scale action recorded."
    }
}

$resourceId = $queue.Id
$shouldScaleUp = $active -ge $ScaleUpThreshold -and (Test-SustainedMetric -ResourceId $resourceId -MetricName "ActiveMessages" -DurationMinutes $SustainedUpMinutes -Threshold $ScaleUpThreshold -Comparison "GreaterOrEqual")
$shouldScaleDown = $active -le $ScaleDownThreshold -and (Test-SustainedMetric -ResourceId $resourceId -MetricName "ActiveMessages" -DurationMinutes $SustainedDownMinutes -Threshold $ScaleDownThreshold -Comparison "LessOrEqual")

if ($shouldScaleUp -and $currentObjective -ne $ScaleUpTarget) {
    Log "Scale-up criteria met."
    Invoke-Scale -TargetObjective $ScaleUpTarget -ResourceGroup $ResourceGroup -ServerName $ServerName -DbName $DbName
    Set-LastScaleTime -VariableName $AutomationVariableName -TimestampUtc (Get-Date).ToUniversalTime()
    return
}

if ($shouldScaleDown -and $currentObjective -ne $ScaleDownTarget) {
    Log "Scale-down criteria met."
    Invoke-Scale -TargetObjective $ScaleDownTarget -ResourceGroup $ResourceGroup -ServerName $ServerName -DbName $DbName
    Set-LastScaleTime -VariableName $AutomationVariableName -TimestampUtc (Get-Date).ToUniversalTime()
    return
}

Log "No scaling action required."