using System;
using AdRev.Core.Services;
using AdRev.Domain.Models;

namespace AdRev.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            try
            {
                string cmd = args[0].ToLower();
                switch (cmd)
                {
                    case "generate":
                        Generate(args);
                        break;
                    case "inspect":
                        Inspect(args);
                        break;
                    case "get-hwid":
                        GetHwid();
                        break;
                    default:
                        PrintHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static void Generate(string[] args)
        {
            // Parse arguments
            string hwid = GetArg(args, "--hwid");
            string typeStr = GetArg(args, "--type") ?? "Lifetime";
            string email = GetArg(args, "--email") ?? "";
            string label = GetArg(args, "--label") ?? "";
            string daysStr = GetArg(args, "--days");

            if (string.IsNullOrEmpty(hwid))
            {
                Console.WriteLine("Error: --hwid is required.");
                Environment.Exit(1);
            }

            if (!Enum.TryParse(typeStr, true, out LicenseType type))
            {
                Console.WriteLine($"Error: Invalid license type '{typeStr}'. Valid values: Lifetime, Annual, Enterprise, Student, Trial.");
                Environment.Exit(1);
            }

            DateTime expiry = DateTime.MaxValue;
            if (type == LicenseType.Annual) expiry = DateTime.UtcNow.AddYears(1);
            else if (type == LicenseType.Trial) expiry = DateTime.UtcNow.AddDays(7);
            
            // Override days if provided
            if (int.TryParse(daysStr, out int days))
            {
                expiry = DateTime.UtcNow.AddDays(days);
            }

            Console.WriteLine("--- License Details ---");
            Console.WriteLine($"Target HWID : {hwid}");
            Console.WriteLine($"License Type: {type}");
            Console.WriteLine($"Label/Tag   : {(string.IsNullOrEmpty(label) ? "N/A" : label)}");
            Console.WriteLine($"Expiry Date : {(expiry == DateTime.MaxValue ? "Lifetime" : expiry.ToShortDateString())}");
            Console.WriteLine("-----------------------");

            // Logic
            var service = new LicensingService();
            string key = service.GenerateLicense(hwid, type, 1, expiry, email, label);

            Console.WriteLine("LICENSE_KEY_START");
            Console.WriteLine(key);
            Console.WriteLine("LICENSE_KEY_END");
        }

        static void Inspect(string[] args)
        {
            string key = GetArg(args, "--key");
            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine("Error: --key is required.");
                Environment.Exit(1);
            }

            var service = new LicensingService();
            var meta = service.InspectLicense(key);

            if (meta == null)
            {
                Console.WriteLine("INVALID LICENSE: Could not decrypt or signature mismatch.");
                Environment.Exit(1);
            }

            Console.WriteLine("--- Decoded License ---");
            Console.WriteLine($"HWID        : {meta.Hwid}");
            Console.WriteLine($"Type        : {meta.Type}");
            Console.WriteLine($"Expiry      : {meta.ExpiryDate}");
            Console.WriteLine($"Max Seats   : {meta.MaxSeats}");
            Console.WriteLine($"Email       : {meta.RegisteredEmail}");
            Console.WriteLine($"Label       : {meta.FeaturesLabel}");
            Console.WriteLine($"Signature   : {meta.Signature}");
            Console.WriteLine("-----------------------");
        }

        static void GetHwid()
        {
            var service = new LicensingService();
            Console.WriteLine(service.GetHardwareId());
        }

        static string? GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }

        static void PrintHelp()
        {
            Console.WriteLine("AdRev License Generator CLI");
            Console.WriteLine("Commands:");
            Console.WriteLine("  generate   : Create a new license key");
            Console.WriteLine("  inspect    : Decode and verify a license key");
            Console.WriteLine("  get-hwid   : Show the HWID of the current machine");
            Console.WriteLine("");
            Console.WriteLine("Usage (Generate):");
            Console.WriteLine("  AdRev.CLI generate --hwid <ID> [--type <Type>] [--email <Email>] [--label <Label>] [--days <Days>]");
            Console.WriteLine("");
            Console.WriteLine("Usage (Inspect):");
            Console.WriteLine("  AdRev.CLI inspect --key <LicenseKey>");
        }
    }
}
