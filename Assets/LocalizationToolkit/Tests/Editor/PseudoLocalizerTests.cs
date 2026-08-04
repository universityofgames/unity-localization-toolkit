using NUnit.Framework;
using UniversityOfGames.LocalizationToolkit.Editor;

namespace UniversityOfGames.LocalizationToolkit.Tests
{
	public class PseudoLocalizerTests
	{
		[Test]
		public void Generate_ReplacesLettersWithAccentedVariants()
		{
			string result = PseudoLocalizer.Generate("abc");
			Assert.That(result, Does.Contain("áƀć"));
			Assert.That(result, Does.Not.Contain("abc"));
		}

		[Test]
		public void Generate_WrapsTextInBrackets()
		{
			string result = PseudoLocalizer.Generate("Hello");
			Assert.That(result, Does.StartWith("⟦"));
			Assert.That(result, Does.EndWith("⟧"));
		}

		[Test]
		public void Generate_PadsTextByAtLeastThirtyPercent()
		{
			const string source = "This is a reasonably long UI string.";
			Assert.That(PseudoLocalizer.Generate(source).Length,
				Is.GreaterThanOrEqualTo((int)(source.Length * 1.3f)));
		}

		[Test]
		public void Generate_PreservesTokenPlaceholders()
		{
			string result = PseudoLocalizer.Generate("Hello {name}, level {level}!");
			Assert.That(result, Does.Contain("{name}"));
			Assert.That(result, Does.Contain("{level}"));
		}

		[Test]
		public void Generate_KeepsDigitsAndPunctuation()
		{
			string result = PseudoLocalizer.Generate("42!");
			Assert.That(result, Does.Contain("42!"));
		}

		[Test]
		public void Generate_WithEmptyText_ReturnsInput()
		{
			Assert.That(PseudoLocalizer.Generate(string.Empty), Is.EqualTo(string.Empty));
			Assert.That(PseudoLocalizer.Generate(null), Is.Null);
		}
	}
}
