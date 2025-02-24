﻿using Application.Dtos;
using Application.QueryServices.ServiceInterfaces;
using Domain.Aggregates.Bus;
using Domain.Aggregates.Ride;
using Domain.Aggregates.Stop;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.ReadRepositories;

public sealed class AppReadDbContext : DbContext, IQueryContext
{
    public AppReadDbContext(DbContextOptions<AppReadDbContext> options) : base(options)
    {
    }

    public IQueryable<T> GetData<T>() where T : class
    {
        return Set<T>().AsNoTracking();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stop>(
            b =>
            {
                b.HasKey(e => e.Id);
                b.OwnsOne(e => e.Position);
            });

        modelBuilder.Entity<Ride>(
            b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.DestinationId);
                b.Property(e => e.DepartureId);
                b.Property(e => e.FirstRecordedStopId);
                b.Property(e => e.TripBegunTime);
                b.Property(e => e.BusId);
                b.Property(e => e.DepartureReachedTime);
            });

        modelBuilder.Entity<Bus>(
            b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.TripId);
                b.Property(e => e.CurrentStopIndex);
                b.Property(e => e.Name);
                b.Property(e => e.LastUpdateTime);
            });

        modelBuilder.Entity<ScheduledStopDto>(
            b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.StopId);
                b.HasIndex(e => e.TripId);
                b.Property(e => e.DepartureTimespan);
                b.Property(e => e.StopSequence);
            });
    }
}