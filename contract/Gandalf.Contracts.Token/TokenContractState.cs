using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Gandalf.Contracts.Token
{
    public class TokenContractState : ContractState
    {
        public SingletonState<Address> Owner { get; set; }

        public MappedState<string, TokenInfo> TokenInfoMap { get; set; }
        public MappedState<Address, string, long> BalanceMap { get; set; }
        public MappedState<Address, Address, string, long> AllowanceMap { get; set; }
    }
}