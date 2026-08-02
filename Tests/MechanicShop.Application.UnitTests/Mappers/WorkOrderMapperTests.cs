using MechanicShop.Tests.Common.Billing;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepaireTasks;
using MechanicShop.Tests.Common.WorkOrders;

using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class WorkOrderMapperTests
{
    private static VehicleModel CreateVehicleModelWithMake()
    {
        var make = VehicleMakeFactory.CreateVehicleMake(Make: "Toyota").Value;
        var model = VehicleModelFactory.CreateVehiclModel(
            model: "Corolla",
            vehicleMake: make).Value;
        return model;
    }

    private static WorkOrder CreateWorkOrder(
        decimal laborCost = 100m,
        decimal partCost = 50m,
        int quantity = 1,
        string repairTaskName = "Brake Inspection",
        bool withInvoice = false)
    {
        var vehicleModel = CreateVehicleModelWithMake();

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModel: vehicleModel).Value]).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var vehicle = customer.vehicles.First();

        var part = PartFactory.CreatePart(
            cost: partCost,
            quantity: quantity).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            name: repairTaskName,
            laborCost: laborCost,
            parts: [part]).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: labor.Id,
            repairTasks: [repairTask]).Value;

        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;

        if (withInvoice)
        {
            var invoiceLine = InvoiceLineItemFactory.CreateInvoiceLineItem(
                unitPrice: repairTask.TotalCost).Value;

            workOrder.Invoice = InvoiceFactory.CreateInvoice(
                workOrderId: workOrder.Id,
                items: [invoiceLine]).Value;
        }

        return workOrder;
    }

    [Fact]
    public void ToDto_WhenWorkOrderIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var workOrder = CreateWorkOrder(
            laborCost: 150,
            partCost: 100,
            quantity: 2,
            withInvoice: true);

        // Act
        var workOrderDto = workOrder.ToDto();

        // Assert
        Assert.Equal(workOrder.Id, workOrderDto.WorkOrderId);
        Assert.Equal(workOrder.Spot, workOrderDto.Spot);
        Assert.Equal(workOrder.StartAtUtc, workOrderDto.StartAtUtc);
        Assert.Equal(workOrder.EndAtUtc, workOrderDto.EndAtUtc);
        Assert.Equal(workOrder.State, workOrderDto.State);
        Assert.Equal(workOrder.CreatedAtUtc, workOrderDto.CreatedAt);
        Assert.NotNull(workOrderDto.Labor);
        Assert.Equal(workOrder.LaborId, workOrderDto.Labor!.LaborId);
        Assert.Equal(workOrder.Labor.FullName, workOrderDto.Labor.Name);
        Assert.NotNull(workOrderDto.Vehicle);
        Assert.Equal(workOrder.Vehicle.Id, workOrderDto.Vehicle!.Id);
        Assert.Equal(workOrder.Vehicle.Year, workOrderDto.Vehicle.Year);
        Assert.Equal(workOrder.Vehicle.LicensePlate, workOrderDto.Vehicle.LicensePlate);
        Assert.Single(workOrderDto.RepairTasks);
        Assert.Equal(workOrder.TotalPartsCost, workOrderDto.TotalPartCost);
        Assert.Equal(workOrder.TotalLaborCost, workOrderDto.TotalLaborCost);
        Assert.Equal(workOrder.Total, workOrderDto.TotalCost);
        Assert.Equal(workOrder.Invoice!.Id, workOrderDto.InvoiceId);
    }

    [Fact]
    public void ToDtos_WhenWorkOrdersAreValid_ShouldMapAllWorkOrders()
    {
        // Arrange
        List<WorkOrder> sourceWorkOrders =
        [
            CreateWorkOrder()
        ];

        // Act
        var workOrderDtos = sourceWorkOrders.ToDto();

        // Assert
        Assert.Single(workOrderDtos);

        var workOrderDto = workOrderDtos.Single();
        var sourceWorkOrder = sourceWorkOrders.Single();

        Assert.Equal(sourceWorkOrder.Id, workOrderDto.WorkOrderId);
        Assert.Equal(sourceWorkOrder.State, workOrderDto.State);
        Assert.Equal(sourceWorkOrder.Vehicle.Id, workOrderDto.Vehicle!.Id);
        Assert.Equal(sourceWorkOrder.Labor.FullName, workOrderDto.Labor!.Name);
    }

    [Fact]
    public void ToListItemDto_WhenWorkOrderIsValid_ShouldMapSummaryProperties()
    {
        // Arrange
        var workOrder = CreateWorkOrder(repairTaskName: "Oil Change");

        // Act
        var listItemDto = workOrder.ToListItemDto();

        // Assert
        Assert.Equal(workOrder.Id, listItemDto.WorkOrderId);
        Assert.Equal(workOrder.Spot, listItemDto.Spot);
        Assert.Equal(workOrder.StartAtUtc, listItemDto.StartAtUtc);
        Assert.Equal(workOrder.EndAtUtc, listItemDto.EndAtUtc);
        Assert.Equal(workOrder.State, listItemDto.State);
        Assert.Equal(workOrder.Labor.FullName, listItemDto.Labor);
        Assert.Single(listItemDto.RepairTasks);
        Assert.Equal("Oil Change", listItemDto.RepairTasks.Single());
    }
}
