/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class Table
    {
        private readonly ITable table;

        private Table(ITable table)
        {
            this.table = table;
        }

        public static Table Standard(int seats)
        {
            return new Table(new StandardTable(seats));
        }

        public static Table Communal(int seats)
        {
            return new Table(new CommunalTable(seats));
        }

        public int Seats
        {
            get { return table.Seats; }
        }

        public bool IsStandard
        {
            get { return table.Accept(new IsStandardVisitor()); }
        }

        public bool IsCommunal
        {
            get { return !IsStandard; }
        }

        public Table WithSeats(int newSeats)
        {
            return table.Accept(new CopyAndUpdateSeatsVisitor(newSeats));
        }

        internal bool Fits(int quantity)
        {
            return quantity <= Seats;
        }

        internal Table Reserve(Reservation reservation)
        {
            return table.Accept(new ReserveVisitor(reservation));
        }

        public override bool Equals(object? obj)
        {
            return obj is Table table &&
                   Seats == table.Seats &&
                   IsStandard == table.IsStandard &&
                   IsCommunal == table.IsCommunal;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Seats, IsStandard, IsCommunal);
        }

        private interface ITable
        {
            int Seats { get; }
            T Accept<T>(ITableVisitor<T> visitor);
        }

        private interface ITableVisitor<T>
        {
            T VisitStandard(int seats, Reservation? reservation);
            T VisitCommunal(
                int seats,
                IReadOnlyCollection<Reservation> reservations);
        }

        private sealed class StandardTable : ITable
        {
            private readonly Reservation? reservation;

            public StandardTable(int seats)
            {
                Seats = seats;
            }

            public StandardTable(int seats, Reservation reservation)
            {
                Seats = seats;
                this.reservation = reservation;
            }

            public int Seats { get; }

            public T Accept<T>(ITableVisitor<T> visitor)
            {
                return visitor.VisitStandard(Seats, reservation);
            }
        }

        private sealed class CommunalTable : ITable
        {
            private readonly int seats;
            private readonly IReadOnlyCollection<Reservation> reservations;

            public CommunalTable(int seats, params Reservation[] reservations)
            {
                this.seats = seats;
                this.reservations = reservations;
            }

            public int Seats {
                get { return seats - reservations.Sum(r => r.Quantity); } 
            }

            public T Accept<T>(ITableVisitor<T> visitor)
            {
                return visitor.VisitCommunal(seats, reservations);
            }
        }

        private sealed class CopyAndUpdateSeatsVisitor : ITableVisitor<Table>
        {
            private readonly int newSeats;

            public CopyAndUpdateSeatsVisitor(int newSeats)
            {
                this.newSeats = newSeats;
            }

            public Table VisitStandard(int seats, Reservation? reservation)
            {
                return new Table(new StandardTable(newSeats));
            }

            public Table VisitCommunal(
                int seats,
                IReadOnlyCollection<Reservation> reservations)
            {
                return new Table(
                    new CommunalTable(newSeats, reservations.ToArray()));
            }
        }

        private sealed class IsStandardVisitor : ITableVisitor<bool>
        {
            public bool VisitStandard(int seats, Reservation? reservation)
            {
                return true;
            }

            public bool VisitCommunal(
                int seats,
                IReadOnlyCollection<Reservation> reservations)
            {
                return false;
            }
        }
        private sealed class ReserveVisitor : ITableVisitor<Table>
        {
            private readonly Reservation reservation;

            public ReserveVisitor(Reservation reservation)
            {
                this.reservation = reservation;
            }

            public Table VisitCommunal(
                int seats,
                IReadOnlyCollection<Reservation> reservations)
            {
                return new Table(
                    new CommunalTable(
                        seats,
                        reservations.Append(reservation).ToArray()));
            }

            public Table VisitStandard(int seats, Reservation? reservation)
            {
                return new Table(new StandardTable(seats, this.reservation));
            }
        }
    }
}