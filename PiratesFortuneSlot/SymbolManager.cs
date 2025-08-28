using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace PiratesFortuneSlot
{
    public enum SymbolType
    {
        Ruby, Sapphire, Emerald, RumBottle, Compass, Map, Parrot, PirateHat, Ship, Wild, Scatter, GoldCoin, Empty
    }

    public class Symbol
    {
        public SymbolType Type { get; set; }
        public Image Image { get; set; }
        public double[] Payouts { get; set; }

        public Symbol(SymbolType type, string imageName, double[] payouts)
        {
            Type = type;
            Image = SymbolManager.GetEmbeddedImage(imageName);
            Payouts = payouts;
        }
    }

    public class SymbolManager
    {
        public List<Symbol> Symbols { get; } = new List<Symbol>();

        public void InitializeSymbols()
        {
            var symbolData = new[]
            {
                new { Type = SymbolType.Ruby, ImageName = "Ruby.png", Payouts = new double[] { 22.40, 44.80, 89.60, 179.20, 358.40, 716.80, 1433.60, 2867.20, 5734.40, 11468.80, 22937.60, 45875.20, 91750.40, 183500.80, 367001.60, 734003.20, 1468006.40 } },
                new { Type = SymbolType.Sapphire, ImageName = "Sapphire.png", Payouts = new double[] { 11.20, 22.40, 44.80, 89.60, 179.20, 358.40, 716.80, 1433.60, 2867.20, 5734.40, 11468.80, 22937.60, 45875.20, 91750.40, 183500.80, 367001.60, 734003.20 } },
                new { Type = SymbolType.Emerald, ImageName = "Emerald.png", Payouts = new double[] { 5.60, 11.20, 22.40, 44.80, 89.60, 179.20, 358.40, 716.80, 1433.60, 2867.20, 5734.40, 11468.80, 22937.60, 45875.20, 91750.40, 183500.80, 367001.60 } },
                new { Type = SymbolType.RumBottle, ImageName = "RumBottle.png", Payouts = new double[] { 0.05, 0.10, 0.20, 0.40, 0.80, 1.60, 3.20, 6.40, 12.80, 25.60, 51.20, 102.40, 204.80, 409.60, 819.20, 1638.40, 3276.80 } },
                new { Type = SymbolType.Compass, ImageName = "Compass.png", Payouts = new double[] { 1.20, 2.40, 4.80, 9.60, 19.20, 38.40, 76.80, 153.60, 307.20, 614.40, 1228.80, 2457.60, 4915.20, 9830.40, 19660.80, 39321.60, 78643.20 } },
                new { Type = SymbolType.Map, ImageName = "Map.png", Payouts = new double[] { 2.80, 5.60, 11.20, 22.40, 44.80, 89.60, 179.20, 358.40, 716.80, 1433.60, 2867.20, 5734.40, 11468.80, 22937.60, 45875.20, 91750.40, 183500.80 } },
                new { Type = SymbolType.Parrot, ImageName = "Parrot.png", Payouts = new double[] { 0.30, 0.60, 1.20, 2.40, 4.80, 9.60, 19.20, 38.40, 76.80, 153.60, 307.20, 614.40, 1228.80, 2457.60, 4915.20, 9830.40, 19660.80 } },
                new { Type = SymbolType.PirateHat, ImageName = "PirateHat.png", Payouts = new double[] { 0.15, 0.30, 0.60, 1.20, 2.40, 4.80, 9.60, 19.20, 38.40, 76.80, 153.60, 307.20, 614.40, 1228.80, 2457.60, 4915.20, 9830.40 } },
                new { Type = SymbolType.Ship, ImageName = "Ship.png", Payouts = new double[] { 0.60, 1.20, 2.40, 4.80, 9.60, 19.20, 38.40, 76.80, 153.60, 307.20, 614.40, 1228.80, 2457.60, 4915.20, 9830.40, 19660.80, 39321.60 } },
                new { Type = SymbolType.Wild, ImageName = "Wild.png", Payouts = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
                new { Type = SymbolType.Scatter, ImageName = "Scatter.png", Payouts = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
                new { Type = SymbolType.GoldCoin, ImageName = "GoldCoin.png", Payouts = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } }
            };

            foreach (var data in symbolData)
            {
                var image = GetEmbeddedImage(data.ImageName);
                if (image != null)
                {
                    Symbols.Add(new Symbol(data.Type, data.ImageName, data.Payouts));
                }
                else
                {
                    MessageBox.Show($"Failed to load image: {data.ImageName}");
                }
            }
        }

        public static Image GetEmbeddedImage(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "PiratesFortuneSlot.Resources." + name;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    MessageBox.Show($"Image resource not found: {resourceName}");
                    return null;
                }
                return Image.FromStream(stream);
            }
        }
    }
}