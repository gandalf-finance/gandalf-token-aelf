using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;

namespace Gandalf.Contracts.Token
{
    public class TokenContractTestBase : DAppContractTestBase<TokenContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        internal TokenContractContainer.TokenContractStub GetTokenContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<TokenContractContainer.TokenContractStub>(DAppContractAddress, senderKeyPair);
        }
    }
}