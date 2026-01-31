using Bogus;
using PropostaService.Domain.Enums;

namespace PropostaService.UnitTests.Builders;

/// <summary>
/// Builder for generating valid proposal test data using Bogus.
/// </summary>
public sealed class ProposalDataBuilder
{
    private readonly Faker _faker = new("pt_BR");
    
    private string _holderCpf = "12345678909";
    private string _holderName = "João Silva";
    private string _holderEmail = "joao.silva@email.com";
    private InsuranceType _insuranceType = InsuranceType.Life;
    private decimal _coverageAmount = 100000m;
    private decimal _premiumAmount = 500m;
    private DateOnly _startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    private DateOnly _endDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
    private string? _description;

    public ProposalDataBuilder WithRandomData()
    {
        _holderCpf = GenerateValidCpf();
        _holderName = _faker.Name.FullName();
        _holderEmail = _faker.Internet.Email();
        _insuranceType = _faker.PickRandom<InsuranceType>();
        _coverageAmount = _faker.Random.Decimal(10000, 1000000);
        _premiumAmount = _faker.Random.Decimal(100, 5000);
        _startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(_faker.Random.Int(1, 30)));
        _endDate = _startDate.AddYears(1);
        _description = _faker.Lorem.Sentence();
        return this;
    }

    public ProposalDataBuilder WithHolderCpf(string cpf)
    {
        _holderCpf = cpf;
        return this;
    }

    public ProposalDataBuilder WithHolderName(string name)
    {
        _holderName = name;
        return this;
    }

    public ProposalDataBuilder WithHolderEmail(string email)
    {
        _holderEmail = email;
        return this;
    }

    public ProposalDataBuilder WithInsuranceType(InsuranceType type)
    {
        _insuranceType = type;
        return this;
    }

    public ProposalDataBuilder WithCoverageAmount(decimal amount)
    {
        _coverageAmount = amount;
        return this;
    }

    public ProposalDataBuilder WithPremiumAmount(decimal amount)
    {
        _premiumAmount = amount;
        return this;
    }

    public ProposalDataBuilder WithCoveragePeriod(DateOnly start, DateOnly end)
    {
        _startDate = start;
        _endDate = end;
        return this;
    }

    public ProposalDataBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public (string HolderCpf, string HolderName, string HolderEmail, InsuranceType InsuranceType,
        decimal CoverageAmount, decimal PremiumAmount, DateOnly StartDate, DateOnly EndDate, string? Description) Build()
    {
        return (_holderCpf, _holderName, _holderEmail, _insuranceType,
            _coverageAmount, _premiumAmount, _startDate, _endDate, _description);
    }

    private static string GenerateValidCpf()
    {
        var random = new Random();
        var cpf = new int[11];

        // Generate first 9 digits
        for (var i = 0; i < 9; i++)
            cpf[i] = random.Next(0, 10);

        // Calculate first check digit
        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += cpf[i] * (10 - i);
        var remainder = sum % 11;
        cpf[9] = remainder < 2 ? 0 : 11 - remainder;

        // Calculate second check digit
        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += cpf[i] * (11 - i);
        remainder = sum % 11;
        cpf[10] = remainder < 2 ? 0 : 11 - remainder;

        return string.Join("", cpf);
    }
}
