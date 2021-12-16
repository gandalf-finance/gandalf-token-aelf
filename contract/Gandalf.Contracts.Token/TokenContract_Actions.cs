using System;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Gandalf.Contracts.Token
{
    public partial class TokenContract
    {
        public override Empty Create(CreateInput input)
        {
            if (State.Owner.Value != null)
            {
                Assert(Context.Sender == State.Owner.Value, "Only owner can create new tokens.");
            }

            Assert(!string.IsNullOrWhiteSpace(input.Symbol), "Invalid symbol.");
            Assert(!string.IsNullOrWhiteSpace(input.TokenName), "Invalid token name.");
            Assert(input.TotalSupply > 0, "Invalid total supply.");
            Assert(State.TokenInfoMap[input.Symbol] == null, $"Token {input.Symbol} already exists.");

            var issuer = input.Issuer ?? Context.Sender;
            var tokenInfo = new TokenInfo
            {
                Symbol = input.Symbol,
                TokenName = input.TokenName,
                Decimals = input.Decimals,
                IsBurnable = input.IsBurnable,
                Issuer = issuer,
                TotalSupply = input.TotalSupply,
                ExternalInfo = input.ExternalInfo
            };
            State.TokenInfoMap[input.Symbol] = tokenInfo;

            Context.Fire(new TokenCreated
            {
                Symbol = tokenInfo.Symbol,
                TokenName = tokenInfo.TokenName,
                Decimals = tokenInfo.Decimals,
                IsBurnable = tokenInfo.IsBurnable,
                Issuer = tokenInfo.Issuer,
                TotalSupply = tokenInfo.TotalSupply,
                ExternalInfo = tokenInfo.ExternalInfo,
            });
            return new Empty();
        }

        public override Empty Issue(IssueInput input)
        {
            Assert(input.To != null, "To address not filled.");
            var tokenInfo = ValidTokenInfo(input.Symbol, input.Amount);
            tokenInfo.Issued = tokenInfo.Issued.Add(input.Amount);
            tokenInfo.Supply = tokenInfo.Supply.Add(input.Amount);

            Assert(tokenInfo.Issued <= tokenInfo.TotalSupply, "Total supply exceeded.");

            State.TokenInfoMap[input.Symbol] = tokenInfo;
            ModifyBalance(input.To, input.Symbol, input.Amount);
            Context.Fire(new Issued
            {
                Symbol = input.Symbol,
                Amount = input.Amount,
                To = input.To,
                Memo = input.Memo
            });
            return new Empty();
        }

        public override Empty Transfer(TransferInput input)
        {
            ValidTokenInfo(input.Symbol, input.Amount);
            DoTransfer(Context.Sender, input.To, input.Symbol, input.Amount, input.Memo);
            DealWithExternalInfoDuringTransfer(new TransferFromInput
            {
                From = Context.Sender,
                To = input.To,
                Amount = input.Amount,
                Symbol = input.Symbol,
                Memo = input.Memo
            });
            return new Empty();
        }

        public override Empty TransferFrom(TransferFromInput input)
        {
            ValidTokenInfo(input.Symbol, input.Amount);
            var allowance = State.AllowanceMap[input.From][Context.Sender][input.Symbol];
            if (allowance < input.Amount)
            {
                Assert(false,
                    $"Insufficient allowance. Token: {input.Symbol}; {allowance}/{input.Amount}.\n" +
                    $"From:{input.From}\tSpender:{Context.Sender}\tTo:{input.To}");
            }

            DoTransfer(input.From, input.To, input.Symbol, input.Amount, input.Memo);
            DealWithExternalInfoDuringTransfer(input);
            State.AllowanceMap[input.From][Context.Sender][input.Symbol] = allowance.Sub(input.Amount);
            return new Empty();
        }

        public override Empty Approve(ApproveInput input)
        {
            ValidTokenInfo(input.Symbol, input.Amount);
            State.AllowanceMap[Context.Sender][input.Spender][input.Symbol] =
                State.AllowanceMap[Context.Sender][input.Spender][input.Symbol].Add(input.Amount);
            Context.Fire(new Approved
            {
                Owner = Context.Sender,
                Spender = input.Spender,
                Symbol = input.Symbol,
                Amount = input.Amount
            });
            return new Empty();
        }

        public override Empty UnApprove(UnApproveInput input)
        {
            ValidTokenInfo(input.Symbol, input.Amount);
            var oldAllowance = State.AllowanceMap[Context.Sender][input.Spender][input.Symbol];
            var amountOrAll = Math.Min(input.Amount, oldAllowance);
            State.AllowanceMap[Context.Sender][input.Spender][input.Symbol] = oldAllowance.Sub(amountOrAll);
            Context.Fire(new UnApproved
            {
                Owner = Context.Sender,
                Spender = input.Spender,
                Symbol = input.Symbol,
                Amount = amountOrAll
            });
            return new Empty();
        }

        public override Empty Burn(BurnInput input)
        {
            var tokenInfo = ValidTokenInfo(input.Symbol, input.Amount);
            Assert(tokenInfo.IsBurnable, "The token is not burnable.");
            ModifyBalance(Context.Sender, input.Symbol, -input.Amount);
            tokenInfo.Supply = tokenInfo.Supply.Sub(input.Amount);
            Context.Fire(new Burned
            {
                Burner = Context.Sender,
                Symbol = input.Symbol,
                Amount = input.Amount
            });
            return new Empty();
        }
    }
}