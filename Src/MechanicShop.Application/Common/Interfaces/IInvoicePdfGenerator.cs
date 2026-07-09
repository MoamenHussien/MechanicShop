public interface IInvoicePdfGenerator
{
    byte [] Generate(Invoice invoice);
}