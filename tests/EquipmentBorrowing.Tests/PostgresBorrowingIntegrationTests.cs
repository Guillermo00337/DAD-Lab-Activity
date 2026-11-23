using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EquipmentBorrowing.Tests;

public sealed class PostgresBorrowingIntegrationTests
    : IClassFixture<PostgresTestFixture>
{
    private readonly PostgresTestFixture _fixture;

    public PostgresBorrowingIntegrationTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(Student Student, Equipment[] Equipment)> CreateTestDataAsync(
        int equipmentCount = 1,
        int maxActiveBorrowings = 3)
    {
        await using var db = await _fixture.ContextFactory.CreateDbContextAsync();

        string unique = Guid.NewGuid().ToString("N");

        var student = new Student(
            0,
            $"TEST-{unique}",
            $"Integration Test Student {unique}",
            true,
            maxActiveBorrowings);

        var equipment = Enumerable.Range(1, equipmentCount)
            .Select(i => new Equipment(
                0,
                $"TEST-{unique}-{i}",
                $"Integration Test Equipment {unique}-{i}"))
            .ToArray();

        db.Students.Add(student);
        db.Equipment.AddRange(equipment);

        await db.SaveChangesAsync();

        return (student, equipment);
    }

    [Fact]
    public async Task CheckoutAsync_WithAvailableEquipment_Succeeds()
    {
        var data = await CreateTestDataAsync();
        var repository = new EfBorrowingRepository(_fixture.ContextFactory);

        var result = await repository.CheckoutAsync(
            data.Student.Id,
            new[] { data.Equipment[0].Id },
            new DateOnly(2026, 11, 22),
            new DateOnly(2026, 11, 29),
            false);

        Assert.True(result.Succeeded);
        Assert.Single(result.Borrowings);

        await using var db = await _fixture.ContextFactory.CreateDbContextAsync();

        var equipment = await db.Equipment.SingleAsync(
            x => x.Id == data.Equipment[0].Id);

        var borrowing = await db.Borrowings.SingleAsync(
            x => x.Id == result.Borrowings[0].Id);

        Assert.False(equipment.IsAvailable);
        Assert.Equal(BorrowingStatus.Active, borrowing.Status);
    }

    [Fact]
    public async Task CheckoutAsync_ExceedingStudentLimit_Fails()
    {
        var data = await CreateTestDataAsync(
            equipmentCount: 2,
            maxActiveBorrowings: 1);

        var repository = new EfBorrowingRepository(_fixture.ContextFactory);

        var result = await repository.CheckoutAsync(
            data.Student.Id,
            data.Equipment.Select(x => x.Id).ToArray(),
            new DateOnly(2026, 11, 22),
            new DateOnly(2026, 11, 29),
            true);

        Assert.False(result.Succeeded);
        Assert.Empty(result.Borrowings);

        await using var db = await _fixture.ContextFactory.CreateDbContextAsync();

        Assert.Equal(
            0,
            await db.Borrowings.CountAsync(
                x => x.StudentId == data.Student.Id));

        var equipment = await db.Equipment
            .Where(x => x.Id == data.Equipment[0].Id ||
                        x.Id == data.Equipment[1].Id)
            .ToListAsync();

        Assert.All(equipment, x => Assert.True(x.IsAvailable));
    }

    [Fact]
    public async Task CheckoutAsync_AlreadyBorrowedEquipment_Fails()
    {
        var data = await CreateTestDataAsync();
        var repository = new EfBorrowingRepository(_fixture.ContextFactory);

        var first = await repository.CheckoutAsync(
            data.Student.Id,
            new[] { data.Equipment[0].Id },
            new DateOnly(2026, 11, 22),
            new DateOnly(2026, 11, 29),
            false);

        Assert.True(first.Succeeded);

        var second = await repository.CheckoutAsync(
            data.Student.Id,
            new[] { data.Equipment[0].Id },
            new DateOnly(2026, 11, 22),
            new DateOnly(2026, 11, 29),
            false);

        Assert.False(second.Succeeded);
        Assert.Empty(second.Borrowings);

        Assert.Equal(
            1,
            await repository.CountActiveByStudentIdAsync(data.Student.Id));
    }

    [Fact]
    public async Task ReturnAsync_ActiveBorrowing_ReturnsEquipment()
    {
        var data = await CreateTestDataAsync();
        var repository = new EfBorrowingRepository(_fixture.ContextFactory);

        var checkout = await repository.CheckoutAsync(
            data.Student.Id,
            new[] { data.Equipment[0].Id },
            new DateOnly(2026, 11, 22),
            new DateOnly(2026, 11, 29),
            false);

        Assert.True(checkout.Succeeded);

        var result = await repository.ReturnAsync(
            checkout.Borrowings[0].Id,
            new DateOnly(2026, 11, 25));

        Assert.True(result.Succeeded);

        await using var db = await _fixture.ContextFactory.CreateDbContextAsync();

        var borrowing = await db.Borrowings.SingleAsync(
            x => x.Id == checkout.Borrowings[0].Id);

        var equipment = await db.Equipment.SingleAsync(
            x => x.Id == data.Equipment[0].Id);

        Assert.Equal(BorrowingStatus.Returned, borrowing.Status);
        Assert.Equal(new DateOnly(2026, 11, 25), borrowing.DateReturned);
        Assert.True(equipment.IsAvailable);
    }

    [Fact]
    public async Task CheckoutAsync_FailureAfterFirstInsert_RollsBackAllChanges()
    {
        var data = await CreateTestDataAsync(equipmentCount: 2);
        var repository = new EfBorrowingRepository(_fixture.ContextFactory);

        string? previousValue =
            Environment.GetEnvironmentVariable(
                "EQUIPMENT_BORROWING_TEST_FAIL_AFTER_FIRST_INSERT");

        try
        {
            Environment.SetEnvironmentVariable(
                "EQUIPMENT_BORROWING_TEST_FAIL_AFTER_FIRST_INSERT",
                "true");

            var result = await repository.CheckoutAsync(
                data.Student.Id,
                data.Equipment.Select(x => x.Id).ToArray(),
                new DateOnly(2026, 11, 22),
                new DateOnly(2026, 11, 29),
                true);

            Assert.False(result.Succeeded);
            Assert.Empty(result.Borrowings);

            await using var db = await _fixture.ContextFactory.CreateDbContextAsync();

            Assert.Equal(
                0,
                await db.Borrowings.CountAsync(
                    x => x.StudentId == data.Student.Id));

            var equipment = await db.Equipment
                .Where(x => x.Id == data.Equipment[0].Id ||
                            x.Id == data.Equipment[1].Id)
                .ToListAsync();

            Assert.All(equipment, x => Assert.True(x.IsAvailable));
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                "EQUIPMENT_BORROWING_TEST_FAIL_AFTER_FIRST_INSERT",
                previousValue);
        }
    }
}
