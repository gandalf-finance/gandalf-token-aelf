namespace Gandalf.Contracts.Token
{
    public partial class TokenContract
    {
        public override GetBalanceOutput GetBalance(GetBalanceInput input)
        {
            var owner = input.Owner ?? Context.Sender;
            return new GetBalanceOutput
            {
                Balance = State.BalanceMap[owner][input.Symbol],
                Owner = owner,
                Symbol = input.Symbol
            };
        }

        public override GetAllowanceOutput GetAllowance(GetAllowanceInput input)
        {
            return new GetAllowanceOutput
            {
                Allowance = State.AllowanceMap[input.Owner][input.Spender][input.Symbol],
                Owner = input.Owner,
                Spender = input.Spender,
                Symbol = input.Symbol
            };
        }

        public override TokenInfo GetTokenInfo(GetTokenInfoInput input)
        {
            return State.TokenInfoMap[input.Symbol];
        }
    }
}