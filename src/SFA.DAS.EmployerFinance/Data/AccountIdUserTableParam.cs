using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;

namespace SFA.DAS.EmployerFinance.Data;

public class AccountIdUserTableParam : SqlMapper.IDynamicParameters
{
    private ICollection<SqlParameter> _additionalParameters { get; }

    private readonly IEnumerable<long> _accountIds;
    public AccountIdUserTableParam(IEnumerable<long> accountIds)
    {
        _accountIds = accountIds;
        _additionalParameters = new List<SqlParameter>();
    }

    public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
    {
        var sqlCommand = (SqlCommand)command;
        sqlCommand.CommandType = CommandType.StoredProcedure;

        var accountList = new List<SqlDataRecord>();

        SqlMetaData[] tvpDefinition = { new("AccountId", SqlDbType.BigInt) };

        foreach (var accountId in _accountIds)
        {
            var rec = new SqlDataRecord(tvpDefinition);
            rec.SetInt64(0, accountId);
            accountList.Add(rec);
        }

        var p = sqlCommand.Parameters.Add("@accountIds", SqlDbType.Structured);
        p.Direction = ParameterDirection.Input;
        p.TypeName = "[employer_financial].[AccountIds]";
        p.Value = accountList;

        sqlCommand.Parameters.AddRange(_additionalParameters.ToArray());
    }

    public void Add(string parameterName, object parameterValue, DbType parameterType)
    {
        var parameter = new SqlParameter(parameterName, parameterType) { Value = parameterValue };

        _additionalParameters.Add(parameter);
    }
}