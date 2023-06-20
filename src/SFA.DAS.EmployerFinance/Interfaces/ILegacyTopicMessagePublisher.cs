namespace SFA.DAS.EmployerFinance.Interfaces;

public interface ILegacyTopicMessagePublisher
{
    Task PublishAsync<T>(T message);
}