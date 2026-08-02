using System.Data.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed class SendWorkOrderCompletedEmailHandler(INotificationService notificationService, IAppDbContext context, ILogger<SendWorkOrderCompletedEmailHandler> logger)
: INotificationHandler<WorkOrderCompleted>
{
    public async Task Handle(WorkOrderCompleted notification, CancellationToken cancellationToken)
    {
        var workOrder = await context.WorkOrders.Include(n => n.Vehicle).ThenInclude(n => n.Customer)
                           .FirstOrDefaultAsync(n => n.Id == notification.WorkOrderId, cancellationToken);
        if (workOrder is null)
        {
            logger.LogError("The Work Order Is Not Found For This Id : {id}", notification.WorkOrderId);
            return;
        }

        var customerName = workOrder.Vehicle.Customer.Name!;

        _ = Task.Run(async () =>
        {
            try
            {
                await notificationService.SendEmailAsync(workOrder.Vehicle.Customer.Email!, customerName, "Your Vehicle Maintenance is Complete", Body(customerName), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email in the background");
            }
        });

        _ = Task.Run(async () =>
        {
            try
            {
                await notificationService.SendSmsAsync(workOrder!.Vehicle.Customer.PhoneNumber!, customerName, Body(customerName), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send SMS in the background");
            }
        });
    }

    private string Body(string CustomerName)
    {
        return @$"Dear Customer : {CustomerName} ,

                        We are pleased to inform you that the maintenance of your vehicle has been successfully completed. 
                        
                        You can now pick up your vehicle at your convenience. Please bring your Work Order reference for a smooth handover.
                        
                        Thank you for choosing our service. We look forward to serving you again.
                        
                        Best regards";
    }
}
