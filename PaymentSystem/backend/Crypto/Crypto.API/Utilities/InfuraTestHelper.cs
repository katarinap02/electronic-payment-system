using Nethereum.Web3;

namespace Crypto.API.Utilities
{
    public class InfuraTestHelper
    {
        public static async Task TestInfuraConnectionAsync(string infuraUrl)
        {
            try
            {
                Console.WriteLine("?? Testing Infura connection...");
                Console.WriteLine($"URL: {infuraUrl}");
                Console.WriteLine();

                var web3 = new Web3(infuraUrl);

                // Test 1: Get latest block number
                Console.WriteLine("Test 1: Getting latest block number...");
                var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                Console.WriteLine($"? Latest block: {blockNumber.Value}");
                Console.WriteLine();

                // Test 2: Get latest block details
                Console.WriteLine("Test 2: Getting block details...");
                var block = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber
                    .SendRequestAsync(Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
                Console.WriteLine($"? Block hash: {block.BlockHash}");
                Console.WriteLine($"? Transactions count: {block.TransactionHashes.Length}");
                Console.WriteLine();

                // Test 3: Get network ID
                Console.WriteLine("Test 3: Getting network ID...");
                var netVersion = await web3.Net.Version.SendRequestAsync();
                Console.WriteLine($"? Network ID: {netVersion}");
                Console.WriteLine($"   (11155111 = Sepolia)");
                Console.WriteLine();

                Console.WriteLine("?? Infura connection successful!");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERROR: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Possible issues:");
                Console.WriteLine("- Invalid API key");
                Console.WriteLine("- Sepolia network not enabled on Infura");
                Console.WriteLine("- Internet connection issue");
            }
        }

        public static async Task TestWalletBalanceAsync(string infuraUrl, string walletAddress)
        {
            try
            {
                Console.WriteLine($"?? Checking balance for: {walletAddress}");
                
                var web3 = new Web3(infuraUrl);
                var balance = await web3.Eth.GetBalance.SendRequestAsync(walletAddress);
                var ethBalance = Web3.Convert.FromWei(balance.Value);

                Console.WriteLine($"? Balance: {ethBalance} ETH");
                Console.WriteLine();

                if (ethBalance < 0.01m)
                {
                    Console.WriteLine("??  Warning: Low balance!");
                    Console.WriteLine("   Get free Sepolia ETH from: https://sepoliafaucet.com/");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERROR: {ex.Message}");
            }
        }
    }
}
