using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Toe.Scripting.Defines
{
    [TestFixture()]
    public class FlatExpressionTestFixture
    {
        [Test]
        public void FlatExpression_Empty_ToString()
        {
            var e = new FlatExpression();

            Assert.AreEqual(PreprocessorExpression.False, e.ToString());
        }
        [Test]
        [TestCase(0ul)]
        [TestCase(1ul)]
        [TestCase(2ul)]
        [TestCase(3ul)]
        public void FlatExpression_Not(ulong mask)
        {
            var e = new FlatExpression(new Operands("A","B"), new FlatExpressionLine[]{new FlatExpressionLine(mask), });

            var not = FlatExpression.Not(e);

            Assert.AreEqual(3, not.Lines.Count);
            foreach (var line in not.Lines)
            {
                Assert.AreNotEqual(line,mask);
            }
        }
    }

    [TestFixture()]
    public class PreprocessorExpressionTestFixture
    {
 
        [Test]
        public void FlatExpression_DefinedValue_ToString()
        {
            var e = PreprocessorExpression.Defined("A").AsFlatExpression();

            Assert.AreEqual("defined(A)", e.ToString());
        }
        [Test]
        public void FlatExpression_NotDefinedValue_ToString()
        {
            var e = PreprocessorExpression.NotDefined("A").AsFlatExpression();

            Assert.AreEqual("!defined(A)", e.ToString());
        }
        [Test]
        public void FlatExpression_SingleDefinedValue_ToTree()
        {
            var e = PreprocessorExpression.Defined("A").AsTreeExpression();

            Assert.AreEqual("defined(A)", e.ToString());
        }
        [Test]
        public void FlatExpression_SingleNotDefinedValue_ToTree()
        {
            var e = PreprocessorExpression.NotDefined("A").AsTreeExpression();

            Assert.AreEqual("!defined(A)", e.ToString());
        }
        [Test]
        [TestCase(1,1,10)]
        [TestCase(2, 2, 10)]
        [TestCase(3, 1<<2, 10)]
        [TestCase(5, 1<<4, 10)]
        [TestCase(10, 400, 10)]
        public void RandomExpression_ConvertFromFlatToTreeAndBack_SameResult(int numOperands, int numRandoms, int randomSeed)
        {
            var operands = new Operands(Enumerable.Range(0, numOperands).Select(_ => (char) ('A' + _)).Select(_ => _.ToString()));
            var rnd = new Random(randomSeed);
            var items = Enumerable.Range(0, numRandoms).Select(_ => (ulong)rnd.Next(0, 1 << numOperands)).Distinct().OrderBy(_=>_).Select(_=>new FlatExpressionLine(_)).ToArray();

            var originalFlat = new FlatExpression(operands, items);
            var tree = originalFlat.AsTreeExpression();
            var flatExpression = originalFlat.AsFlatExpression();

            if (numOperands <= 5)
            {
                Console.WriteLine(originalFlat);
                Console.WriteLine(tree);
                Console.WriteLine(flatExpression);
            }

            Assert.AreEqual(originalFlat.Lines.Count, flatExpression.Lines.Count);
            for (var index = 0; index < flatExpression.Lines.Count; index++)
            {
                Assert.AreEqual(originalFlat.Lines[index], flatExpression.Lines[index]);
            }
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(10)]
        public void And(int numOperands)
        {
            var rnd = new Random(10);
            var operandsA = new Operands(Enumerable.Range(0, numOperands).Select(_ => (char)('A' + rnd.Next(0, numOperands*2))).Distinct().OrderBy(_=>_).Select(_ => _.ToString()));
            var operandsB = new Operands(Enumerable.Range(0, numOperands).Select(_ => (char)('A' + rnd.Next(0, numOperands * 2))).Distinct().OrderBy(_ => _).Select(_ => _.ToString()));
            var itemsA = Enumerable.Range(0, 1<< (numOperands-1)).Select(_ => (ulong)rnd.Next(0, 1 << operandsA.Count)).Distinct().OrderBy(_ => _).Select(_ => new FlatExpressionLine(_)).ToArray();
            var itemsB = Enumerable.Range(0, 1 << (numOperands - 1)).Select(_ => (ulong)rnd.Next(0, 1 << operandsB.Count)).Distinct().OrderBy(_ => _).Select(_ => new FlatExpressionLine(_)).ToArray();

            var a = new FlatExpression(operandsA, itemsA);
            var b = new FlatExpression(operandsB, itemsB);

            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(a.And(b));
            Console.WriteLine(a.Or(b));
            Console.WriteLine(new TruthTable(a.Or(b)));
        }
    }
}
