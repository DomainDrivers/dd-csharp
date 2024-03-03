using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Risk;
using DomainDrivers.SmartSchedule.Shared;
using NSubstitute;
using static DomainDrivers.SmartSchedule.Shared.Capability;
using static DomainDrivers.SmartSchedule.Risk.RiskPeriodicCheckSagaStep;

namespace DomainDrivers.SmartSchedule.Tests.Risk;

public class RiskPeriodicCheckSagaTest
{
    private static readonly TimeProvider Clock = Substitute.For<TimeProvider>();
    private static readonly Capability Java = Skill("JAVA");
    private static readonly TimeSlot OneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2022, 1, 1);
    private static readonly Demands SingleDemand = Demands.Of(new Demand(Java, OneDay));
    private static readonly Demands ManyDemands = Demands.Of(new Demand(Java, OneDay), new Demand(Java, OneDay));

    private static readonly TimeSlot ProjectDates = new TimeSlot(DateTime.Parse("2021-01-01T00:00:00.00Z"),
        DateTime.Parse("2021-01-05T00:00:00.00Z"));

    private static readonly ProjectAllocationsId ProjectId = ProjectAllocationsId.NewOne();
    private static readonly AllocatableCapabilityId CapabilityId = AllocatableCapabilityId.NewOne();

    public RiskPeriodicCheckSagaTest()
    {
        Clock.GetUtcNow().Returns(DateTimeOffset.Now);
    }

    [Fact]
    public void UpdatesInitialDemandsOnSagaCreation()
    {
        //when
        var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);

        //then
        Assert.Equal(SingleDemand, saga.MissingDemands);
    }

    [Fact]
    public void UpdatesDeadlineOnDeadlineSet()
    {
        //given
        var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);
        //and
        saga.Handle(new ProjectAllocationScheduled(ProjectId, ProjectDates, Clock.GetUtcNow().DateTime));

        //then
        Assert.Equal(ProjectDates.To, saga.Deadline);
    }

    [Fact]
    public void UpdatesDemandsOnScheduleChange()
    {
        //given
        var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);

        //when
        var nextStep =
            saga.Handle(new ProjectAllocationsDemandsScheduled(ProjectId, ManyDemands, Clock.GetUtcNow().DateTime));

        //then
        Assert.Equal(DoNothing, nextStep);
        Assert.Equal(ManyDemands, saga.MissingDemands);
    }

    [Fact]
    public void UpdatedEarningsOnEarningsRecalculated()
    {
        //given
        var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);

        //when
        var nextStep = saga.Handle(new EarningsRecalculated(ProjectId, Earnings.Of(1000), Clock.GetUtcNow().DateTime));
        Assert.Equal(DoNothing, nextStep);

        //then
        Assert.Equal(Earnings.Of(1000), saga.Earnings);

        //when
        nextStep = saga.Handle(new EarningsRecalculated(ProjectId, Earnings.Of(900), Clock.GetUtcNow().DateTime));

        //then
        Assert.Equal(Earnings.Of(900), saga.Earnings);
        Assert.Equal(DoNothing, nextStep);
    }
    
    [Fact]
        public void InformsAboutDemandsSatisfiedWhenDemandsRescheduled()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, ManyDemands);
            //and
            saga.Handle(new EarningsRecalculated(ProjectId, Earnings.Of(1000), Clock.GetUtcNow().DateTime));

            //when
            var stillMissing = saga.Handle(new ProjectAllocationsDemandsScheduled(ProjectId, SingleDemand, Clock.GetUtcNow().DateTime));
            var zeroDemands = saga.Handle(new ProjectAllocationsDemandsScheduled(ProjectId, Demands.None(), Clock.GetUtcNow().DateTime));

            //then
            Assert.Equal(DoNothing, stillMissing);
            Assert.Equal(NotifyAboutDemandsSatisfied, zeroDemands);
        }

        [Fact]
        public void NotifyAboutNoMissingDemandsOnCapabilityAllocated()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);

            //when
            var nextStep = saga.Handle(new CapabilitiesAllocated(Guid.NewGuid(), ProjectId, Demands.None(), Clock.GetUtcNow().DateTime));

            //then
            Assert.Equal(RiskPeriodicCheckSagaStep.NotifyAboutDemandsSatisfied, nextStep);
        }

        [Fact]
        public void NoNewStepsOnCapabilityAllocatedWhenMissingDemands()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, ManyDemands);

            //when
            var nextStep = saga.Handle(new CapabilitiesAllocated(Guid.NewGuid(), ProjectId, SingleDemand, Clock.GetUtcNow().DateTime));

            //then
            Assert.Equal(DoNothing, nextStep);
        }

        [Fact]
        public void DoNothingOnResourceTakenOverWhenAfterDeadline()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, ManyDemands);
            //and
            saga.Handle(new CapabilitiesAllocated(Guid.NewGuid(), ProjectId, SingleDemand, Clock.GetUtcNow().DateTime));
            //and
            saga.Handle(new ProjectAllocationScheduled(ProjectId, ProjectDates, Clock.GetUtcNow().DateTime));

            //when
            var afterDeadline = ProjectDates.To.AddHours(100);
            var nextStep = saga.Handle(new ResourceTakenOver(CapabilityId.ToAvailabilityResourceId(), new HashSet<Owner> { Owner.Of(ProjectId.Id) }, OneDay, afterDeadline));

            //then
            Assert.Equal(DoNothing, nextStep);
        }

        [Fact]
        public void NotifyAboutRiskOnResourceTakenOverWhenBeforeDeadline()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, ManyDemands);
            //and
            saga.Handle(new CapabilitiesAllocated(Guid.NewGuid(), ProjectId, ManyDemands, Clock.GetUtcNow().DateTime));
            //and
            saga.Handle(new ProjectAllocationScheduled(ProjectId, ProjectDates, Clock.GetUtcNow().DateTime));

            //when
            var beforeDeadline = ProjectDates.To.AddHours(-100);
            var nextStep = saga.Handle(new ResourceTakenOver(CapabilityId.ToAvailabilityResourceId(), new HashSet<Owner> { Owner.Of(ProjectId.Id) }, OneDay, beforeDeadline));

            //then
            Assert.Equal(RiskPeriodicCheckSagaStep.NotifyAboutPossibleRisk, nextStep);
        }

        [Fact]
        public void NoNextStepOnCapabilityReleased()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);
            //and
            saga.Handle(new CapabilitiesAllocated(Guid.NewGuid(), ProjectId, Demands.None(), Clock.GetUtcNow().DateTime));

            //when
            var nextStep = saga.Handle(new CapabilityReleased(ProjectId, SingleDemand, Clock.GetUtcNow().DateTime));

            //then
            Assert.Equal(DoNothing, nextStep);
        }

        [Fact]
        public void WeeklyCheckShouldResultInNothingWhenAllDemandsSatisfied()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);
            //and
            saga.Handle(new EarningsRecalculated(ProjectId, Earnings.Of(1000), Clock.GetUtcNow().DateTime));
            //and
            saga.Handle(new CapabilitiesAllocated(Guid.NewGuid(), ProjectId, Demands.None(), Clock.GetUtcNow().DateTime));
            //and
            saga.Handle(new ProjectAllocationScheduled(ProjectId, ProjectDates, Clock.GetUtcNow().DateTime));

            //when
            var wayBeforeDeadline = ProjectDates.To.AddDays(-1);
            var nextStep = saga.HandleWeeklyCheck(wayBeforeDeadline);

            //then
            Assert.Equal(DoNothing, nextStep);
        }

        [Fact]
        public void WeeklyCheckShouldResultInNothingWhenAfterDeadline()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);
            //and
            saga.Handle(new EarningsRecalculated(ProjectId, Earnings.Of(1000), Clock.GetUtcNow().DateTime));
            //and
            saga.Handle(new ProjectAllocationScheduled(ProjectId, ProjectDates, Clock.GetUtcNow().DateTime));

            //when
            var wayAfterDeadline = ProjectDates.To.AddDays(300);
            var nextStep = saga.HandleWeeklyCheck(wayAfterDeadline);

            //then
            Assert.Equal(DoNothing, nextStep);
        }

        [Fact]
        public void WeeklyCheckDoesNothingWhenNoDeadline()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);

            //when
            var nextStep = saga.HandleWeeklyCheck(Clock.GetUtcNow().DateTime);

            //then
            Assert.Equal(DoNothing, nextStep);
        }

        [Fact]
        public void WeeklyCheckShouldResultInNothingWhenNotCloseToDeadlineAndDemandsNotSatisfied()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, SingleDemand);
            //and
            saga.Handle(new EarningsRecalculated(ProjectId, Earnings.Of(1000), Clock.GetUtcNow().DateTime));
            //and
            saga.Handle(new ProjectAllocationScheduled(ProjectId, ProjectDates, Clock.GetUtcNow().DateTime));

            //when
            var wayBeforeDeadline = ProjectDates.To.AddDays(-300);
            var nextStep = saga.HandleWeeklyCheck(wayBeforeDeadline);

            //then
            Assert.Equal(DoNothing, nextStep);
        }

        [Fact]
        public void WeeklyCheckShouldResultInFindAvailableWhenCloseToDeadlineAndDemandsNotSatisfied()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, ManyDemands);
            //and
            saga.Handle(new EarningsRecalculated(ProjectId, Earnings.Of(1000), Clock.GetUtcNow().DateTime));
            //and
            saga.Handle(new ProjectAllocationScheduled(ProjectId, ProjectDates, Clock.GetUtcNow().DateTime));

            //when
            var closeToDeadline = ProjectDates.To.AddDays(-20);
            var nextStep = saga.HandleWeeklyCheck(closeToDeadline);

            //then
            Assert.Equal(FindAvailable, nextStep);
        }

        [Fact]
        public void WeeklyCheckShouldResultInReplacementSuggestingWhenHighValueProjectReallyCloseToDeadlineAndDemandsNotSatisfied()
        {
            //given
            var saga = new RiskPeriodicCheckSaga(ProjectId, ManyDemands);
            //and
            saga.Handle(new EarningsRecalculated(ProjectId, Earnings.Of(10000), Clock.GetUtcNow().DateTime));
            //and
            saga.Handle(new ProjectAllocationScheduled(ProjectId, ProjectDates, Clock.GetUtcNow().DateTime));

            //when
            var reallyCloseToDeadline = ProjectDates.To.AddDays(-2);
            var nextStep = saga.HandleWeeklyCheck(reallyCloseToDeadline);

            //then
            Assert.Equal(SuggestReplacement, nextStep);
        }
}