﻿using SFA.DAS.EAS.Infrastructure.Extensions;
using System;
using System.Text;

namespace SFA.DAS.EAS.Infrastructure.Exceptions.MessageFormatters
{
    internal class ExceptionMessageFormatter : BaseExceptionMessageFormatter
    {
        protected override void CreateFormattedMessage(Exception exception, StringBuilder messageBuilder)
        {
            if (exception == null)
            {
                messageBuilder.AppendLine("Exception is null");
            }
            else
            {
                messageBuilder.AppendLine($"Exception: {exception.GetType().Name}");
                messageBuilder.AppendLine($"Message: {exception.Message}");

                exception.InnerException?.AppendMessage(messageBuilder);
            }
        }
    }
}