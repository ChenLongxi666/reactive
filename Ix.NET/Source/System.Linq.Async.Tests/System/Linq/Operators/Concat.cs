﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class Concat : AsyncEnumerableTests
    {
        [Fact]
        public void Concat_Null()
        {
            AssertThrows<ArgumentNullException>(() => AsyncEnumerable.Concat<int>(null, AsyncEnumerable.Return(42)));
            AssertThrows<ArgumentNullException>(() => AsyncEnumerable.Concat<int>(AsyncEnumerable.Return(42), null));
            AssertThrows<ArgumentNullException>(() => AsyncEnumerable.Concat<int>(default(IAsyncEnumerable<int>[])));
            AssertThrows<ArgumentNullException>(() => AsyncEnumerable.Concat<int>(default(IEnumerable<IAsyncEnumerable<int>>)));
        }

        [Fact]
        public void Concat1()
        {
            var ys = new[] { 1, 2, 3 }.ToAsyncEnumerable().Concat(new[] { 4, 5, 6 }.ToAsyncEnumerable());

            var e = ys.GetAsyncEnumerator();
            HasNext(e, 1);
            HasNext(e, 2);
            HasNext(e, 3);
            HasNext(e, 4);
            HasNext(e, 5);
            HasNext(e, 6);
            NoNext(e);
        }

        [Fact]
        public void Concat2()
        {
            var ex = new Exception("Bang");
            var ys = new[] { 1, 2, 3 }.ToAsyncEnumerable().Concat(AsyncEnumerable.Throw<int>(ex));

            var e = ys.GetAsyncEnumerator();
            HasNext(e, 1);
            HasNext(e, 2);
            HasNext(e, 3);
            AssertThrows(() => e.MoveNextAsync().Wait(WaitTimeoutMs), (Exception ex_) => ((AggregateException)ex_).Flatten().InnerExceptions.Single() == ex);
        }

        [Fact]
        public void Concat3()
        {
            var ex = new Exception("Bang");
            var ys = AsyncEnumerable.Throw<int>(ex).Concat(new[] { 4, 5, 6 }.ToAsyncEnumerable());

            var e = ys.GetAsyncEnumerator();
            AssertThrows(() => e.MoveNextAsync().Wait(WaitTimeoutMs), (Exception ex_) => ((AggregateException)ex_).Flatten().InnerExceptions.Single() == ex);
        }

        [Fact]
        public void Concat4()
        {
            var xs = new[] { 1, 2, 3 }.ToAsyncEnumerable();
            var ys = new[] { 4, 5 }.ToAsyncEnumerable();
            var zs = new[] { 6, 7, 8 }.ToAsyncEnumerable();

            var res = AsyncEnumerable.Concat(xs, ys, zs);

            var e = res.GetAsyncEnumerator();
            HasNext(e, 1);
            HasNext(e, 2);
            HasNext(e, 3);
            HasNext(e, 4);
            HasNext(e, 5);
            HasNext(e, 6);
            HasNext(e, 7);
            HasNext(e, 8);
            NoNext(e);
        }

        [Fact]
        public void Concat5()
        {
            var ex = new Exception("Bang");
            var xs = new[] { 1, 2, 3 }.ToAsyncEnumerable();
            var ys = new[] { 4, 5 }.ToAsyncEnumerable();
            var zs = AsyncEnumerable.Throw<int>(ex);

            var res = AsyncEnumerable.Concat(xs, ys, zs);

            var e = res.GetAsyncEnumerator();
            HasNext(e, 1);
            HasNext(e, 2);
            HasNext(e, 3);
            HasNext(e, 4);
            HasNext(e, 5);
            AssertThrows(() => e.MoveNextAsync().Wait(WaitTimeoutMs), (Exception ex_) => ((AggregateException)ex_).Flatten().InnerExceptions.Single() == ex);
        }

        [Fact]
        public void Concat6()
        {
            var res = AsyncEnumerable.Concat(ConcatXss());

            var e = res.GetAsyncEnumerator();
            HasNext(e, 1);
            HasNext(e, 2);
            HasNext(e, 3);
            HasNext(e, 4);
            HasNext(e, 5);
            AssertThrows(() => e.MoveNextAsync().Wait(WaitTimeoutMs), (Exception ex_) => ((AggregateException)ex_).Flatten().InnerExceptions.Single().Message == "Bang!");
        }

        [Fact]
        public void Concat7()
        {
            var ws = new[] { 1, 2, 3 }.ToAsyncEnumerable();
            var xs = new[] { 4, 5 }.ToAsyncEnumerable();
            var ys = new[] { 6, 7, 8 }.ToAsyncEnumerable();
            var zs = new[] { 9, 10, 11 }.ToAsyncEnumerable();

            var res = ws.Concat(xs).Concat(ys).Concat(zs);

            var e = res.GetAsyncEnumerator();
            HasNext(e, 1);
            HasNext(e, 2);
            HasNext(e, 3);
            HasNext(e, 4);
            HasNext(e, 5);
            HasNext(e, 6);
            HasNext(e, 7);
            HasNext(e, 8);
            HasNext(e, 9);
            HasNext(e, 10);
            HasNext(e, 11);
            NoNext(e);
        }

        [Fact]
        public async Task Concat8()
        {
            var ws = new[] { 1, 2, 3 }.ToAsyncEnumerable();
            var xs = new[] { 4, 5 }.ToAsyncEnumerable();
            var ys = new[] { 6, 7, 8 }.ToAsyncEnumerable();
            var zs = new[] { 9, 10, 11 }.ToAsyncEnumerable();

            var res = ws.Concat(xs).Concat(ys).Concat(zs);

            await SequenceIdentity(res);
        }

        [Fact]
        public async Task Concat9()
        {
            var xs = new[] { 1, 2, 3 }.ToAsyncEnumerable();
            var ys = new[] { 4, 5 }.ToAsyncEnumerable();
            var zs = new[] { 6, 7, 8 }.ToAsyncEnumerable();

            var res = AsyncEnumerable.Concat(xs, ys, zs);

            await SequenceIdentity(res);
        }

        [Fact]
        public async Task Concat10()
        {
            var xs = new[] { 1, 2, 3 }.ToAsyncEnumerable();
            var ys = new[] { 4, 5 }.ToAsyncEnumerable();
            var zs = new[] { 6, 7, 8 }.ToAsyncEnumerable();

            var c = xs.Concat(ys).Concat(zs);

            var res = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            Assert.True(res.SequenceEqual(await c.ToArray()));
        }

        [Fact]
        public async Task Concat11()
        {
            var xs = new[] { 1, 2, 3 }.ToAsyncEnumerable();
            var ys = new[] { 4, 5 }.ToAsyncEnumerable();
            var zs = new[] { 6, 7, 8 }.ToAsyncEnumerable();

            var c = xs.Concat(ys).Concat(zs);

            var res = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            Assert.True(res.SequenceEqual(await c.ToList()));
        }

        [Fact]
        public async Task Concat12()
        {
            var xs = new[] { 1, 2, 3 }.ToAsyncEnumerable();
            var ys = new[] { 4, 5 }.ToAsyncEnumerable();
            var zs = new[] { 6, 7, 8 }.ToAsyncEnumerable();

            var c = xs.Concat(ys).Concat(zs);

            Assert.Equal(8, await c.Count());
        }

        static IEnumerable<IAsyncEnumerable<int>> ConcatXss()
        {
            yield return new[] { 1, 2, 3 }.ToAsyncEnumerable();
            yield return new[] { 4, 5 }.ToAsyncEnumerable();
            throw new Exception("Bang!");
        }
    }
}
