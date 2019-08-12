using Pegasus_backend.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Pegasus_backend.Services
{
    public class InvoicePDFGeneratorService
    {
        private readonly Invoice _invoice;
        private readonly ILogger _log;

        public InvoicePDFGeneratorService(Invoice invoice, ILogger<Object> log)
        {
            _invoice = invoice;
            _log = log;
        }

        public void SavePDF()
        {
            var infos = new List<InvoicePdfGeneratorModel>
            {
                new InvoicePdfGeneratorModel
                {
                    title = "Lesson Fee",
                    amount = _invoice.LessonFee.GetValueOrDefault()
                },
                new InvoicePdfGeneratorModel
                {
                    title = "Total Fee",
                    amount = _invoice.TotalFee.GetValueOrDefault()
                },
                new InvoicePdfGeneratorModel
                {
                    title = "Paid Fee",
                    amount = _invoice.PaidFee.GetValueOrDefault()
                },
                new InvoicePdfGeneratorModel
                {
                    title = "Owing Fee",
                    amount = _invoice.OwingFee.GetValueOrDefault()
                },
            };
            if (_invoice.ConcertFee.HasValue)
            {
                infos.Add(new InvoicePdfGeneratorModel
                {
                    title = _invoice.ConcertFeeName,
                    amount = _invoice.ConcertFee.GetValueOrDefault()
                });
            }
            if (_invoice.NoteFee.HasValue)
            {
                infos.Add(new InvoicePdfGeneratorModel
                {
                    title = _invoice.LessonNoteFeeName,
                    amount = _invoice.NoteFee.GetValueOrDefault()
                });
            }
            if (_invoice.Other1Fee.HasValue)
            {
                infos.Add(new InvoicePdfGeneratorModel
                {
                    title = _invoice.Other1FeeName,
                    amount = _invoice.Other1Fee.GetValueOrDefault()
                });
            }
            if (_invoice.Other2Fee.HasValue)
            {
                infos.Add(new InvoicePdfGeneratorModel
                {
                    title = _invoice.Other2FeeName,
                    amount = _invoice.Other2Fee.GetValueOrDefault()
                });
            }
            if (_invoice.Other3Fee.HasValue)
            {
                infos.Add(new InvoicePdfGeneratorModel
                {
                    title = _invoice.Other3FeeName,
                    amount = _invoice.Other3Fee.GetValueOrDefault()
                });
            }

            InvoiceGenerator("Title", "Name", _invoice.TotalFee.GetValueOrDefault(), infos, "TestInvoiceGenerator");
        }

        private void InvoiceGenerator(string invoiceTitle, string invoiceName, decimal invoiceAmount, List<InvoicePdfGeneratorModel> infos, string filename)
        {
            //title
            PdfPTable title = new PdfPTable(1);
            title.WidthPercentage = 80;
            title.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
            title.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
            title.DefaultCell.BorderWidth = 0;
            Chunk titleChunk = new Chunk(invoiceTitle, FontFactory.GetFont("Times New Roman"));
            titleChunk.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
            titleChunk.Font.SetStyle(0);
            titleChunk.Font.Size = 18;
            Phrase titlePhrase = new Phrase();
            titlePhrase.Add(titleChunk);
            title.AddCell(titlePhrase);

            //blank
            PdfPTable pdfTableBlank = new PdfPTable(1);
            pdfTableBlank.DefaultCell.BorderWidth = 0;
            pdfTableBlank.DefaultCell.Border = 0;
            pdfTableBlank.AddCell(new Phrase(" "));

            //invoice to (who)
            PdfPTable name = new PdfPTable(1);
            name.WidthPercentage = 80;
            name.DefaultCell.BorderWidth = 0;
            Chunk nameChunk = new Chunk("Invoice To: " + invoiceName, FontFactory.GetFont("Times New Roman"));
            nameChunk.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
            nameChunk.Font.SetStyle(0);
            nameChunk.Font.Size = 10;
            Phrase namePhrase = new Phrase();
            namePhrase.Add(nameChunk);
            name.AddCell(namePhrase);

            //detail table
            PdfPTable table = new PdfPTable(2);
            table.DefaultCell.Padding = 5;
            table.WidthPercentage = 80;
            table.DefaultCell.BorderWidth = 0.5f;
            table.AddCell(new Phrase("Title"));
            table.AddCell(new Phrase("Amount"));
            infos.ForEach(s =>
            {
                table.AddCell(s.title);
                table.AddCell("$ " + s.amount);
            });

            //Total Amount
            PdfPTable amount = new PdfPTable(1);
            amount.WidthPercentage = 80;
            amount.DefaultCell.BorderWidth = 0;
            Chunk amountchunk = new Chunk("Total Amount: $ " + invoiceAmount, FontFactory.GetFont("Times New Roman"));
            amountchunk.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
            amountchunk.Font.SetStyle(0);
            amountchunk.Font.Size = 14;
            Phrase amountPhrase = new Phrase();
            amountPhrase.Add(amountchunk);
            amount.AddCell(amountPhrase);


            var filenameToKeep = filename + ".pdf";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "invoice", filenameToKeep);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                pdfDoc.Add(title);
                pdfDoc.Add(pdfTableBlank);
                pdfDoc.Add(name);
                pdfDoc.Add(pdfTableBlank);
                pdfDoc.Add(table);
                pdfDoc.Add(pdfTableBlank);
                pdfDoc.Add(amount);
                pdfDoc.Close();
                stream.Close();
            }
        }
    }
}
