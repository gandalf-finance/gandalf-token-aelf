using Google.Protobuf.WellKnownTypes;

namespace Gandalf.Contracts.Token
{
    public partial class TokenContract : TokenContractContainer.TokenContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            Assert(State.Owner.Value == null, "Already initialized.");
            State.Owner.Value = input.Owner;
            return new Empty();
        }
    }
}