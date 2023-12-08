// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Lists;

namespace osu.Framework.Tests.Bindables
{
    [TestFixture]
    [NonParallelizable]
    public class BindableThreadSafetyTest
    {
        private const int repeat_count = 5;
        private static readonly int num_bindables = Environment.ProcessorCount;

        /// <summary>
        /// Checks thread safety of <see cref="Bindable{T}.Bindings"/> weak list creation in <see cref="Bindable{T}.BindTo"/>.
        /// </summary>
        [Test]
        [Repeat(repeat_count)]
        public void TestBindingsCreationThreadSafety([Values] bool parallel)
        {
            var underTest = new TestBindable();
            var bindables = Enumerable.Range(0, num_bindables).Select(_ => new TestBindable()).ToArray();

            Assume.That(num_bindables > 1);
            Assume.That(underTest.Bindings, Is.Null);

            if (parallel)
            {
                Parallel.ForEach(bindables, b =>
                {
                    underTest.BindTo(b);
                });
            }
            else
            {
                // also run the algorithm sequentially to show that the assumptions are correct
                foreach (var b in bindables)
                {
                    underTest.BindTo(b);
                }
            }

            Assert.That(underTest.Bindings, Is.Not.Null);
            Assert.That(underTest.Bindings.Count(), Is.EqualTo(num_bindables));

            underTest.Value = 42;
            Assert.That(bindables.Select(b => b.Value), Has.All.EqualTo(underTest.Value));
        }

        [Test]
        [Repeat(repeat_count)]
        public void TestBindableListBindingsCreationThreadSafety([Values] bool parallel)
        {
            var underTest = new BindableList<int>();
            var bindables = Enumerable.Range(0, num_bindables).Select(_ => new BindableList<int>()).ToArray();

            Assume.That(num_bindables > 1);

            if (parallel)
            {
                Parallel.ForEach(bindables, b =>
                {
                    underTest.BindTo(b);
                });
            }
            else
            {
                // also run the algorithm sequentially to show that the assumptions are correct
                foreach (var b in bindables)
                {
                    underTest.BindTo(b);
                }
            }

            int[] items = { 42 };

            underTest.AddRange(items);
            Assert.That(bindables, Has.All.EqualTo(items));
        }

        [Test]
        [Repeat(repeat_count)]
        public void TestBindableDictionaryBindingsCreationThreadSafety([Values] bool parallel)
        {
            var underTest = new BindableDictionary<string, int>();
            var bindables = Enumerable.Range(0, num_bindables).Select(_ => new BindableDictionary<string, int>()).ToArray();

            Assume.That(num_bindables > 1);

            if (parallel)
            {
                Parallel.ForEach(bindables, b =>
                {
                    underTest.BindTo(b);
                });
            }
            else
            {
                // also run the algorithm sequentially to show that the assumptions are correct
                foreach (var b in bindables)
                {
                    underTest.BindTo(b);
                }
            }

            underTest.Add("key", 42);
            Assert.That(bindables, Has.All.ContainKey("key").And.ContainValue(42));
        }

        private class TestBindable : Bindable<int>
        {
            public new LockedWeakList<Bindable<int>> Bindings => base.Bindings;
        }
    }
}
