﻿using Checkers.Core.Board;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Checkers.Core.Rules
{
    public class MoveSequence : IEnumerable<MoveStep>
    {
        private LinkedList<MoveStep> _sequence = new LinkedList<MoveStep>();
        private HashSet<Point> _set = new HashSet<Point>();

        internal MoveSequence() { }
        internal MoveSequence(params MoveStep[] steps) : this()
        {
            foreach(var step in steps) Add(step);
        }

        internal MoveSequence(MoveSequence sequence) : this()
        {
            _set = new HashSet<Point>(sequence._set);
            _sequence = new LinkedList<MoveStep>(sequence._sequence);
        }

        internal void Add(MoveStep p)
        {
            _sequence.AddLast(p);
            _set.Add(p.Target);
        }

        public bool Contains(Point p) => _set.Contains(p);

        public bool IsEmpty => _sequence.Count == 0;

        public IEnumerator<MoveStep> GetEnumerator() => _sequence.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _sequence.GetEnumerator();

        public override string ToString() => $"Sequence({String.Join("->", _sequence.ToArray())})";

        public override bool Equals(object obj) => obj is MoveSequence m && Equals(m);

        public bool Equals(MoveSequence other) => _set.SetEquals(other._set);

        public override int GetHashCode()
        {
            throw new NotImplementedException(); //TODO: implement this
        }

        public static bool operator ==(MoveSequence left, MoveSequence right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MoveSequence left, MoveSequence right)
        {
            return !(left == right);
        }
    }

}
