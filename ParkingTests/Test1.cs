using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PriceCalculatorTests
{
    [TestMethod]
    public void TioMinuterArGratis()
    {
        PriceCalculator calculator = new PriceCalculator();

        decimal pris = calculator.CalculatePrice(
            TimeSpan.FromMinutes(10), 10, 20m);

        Assert.AreEqual(0m, pris);
    }

    [TestMethod]
    public void OverTioMinuterKostarEnTimme()
    {
        PriceCalculator calculator = new PriceCalculator();

        decimal pris = calculator.CalculatePrice(
            TimeSpan.FromSeconds(601), 10, 20m);

        Assert.AreEqual(20m, pris);
    }

    [TestMethod]
    public void SextioenMinuterKostarTvaTimmar()
    {
        PriceCalculator calculator = new PriceCalculator();

        decimal pris = calculator.CalculatePrice(
            TimeSpan.FromMinutes(61), 10, 20m);

        Assert.AreEqual(40m, pris);
    }
}