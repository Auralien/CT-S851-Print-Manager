# CT-S851-Print-Manager

This is a C# manager class for printing with **Citizen CT-S851** via ESC/POS commands over a network.

Since this project was created as a part of commercial application for Russian customer, only Russian PC866 codepage is supported at the moment. Other codepages support could be added in future updates.

## Usage

Just add this class to your C# project, add `using PrintManager;` derective where it is needed.

Then create object with `CT_S851_Manager` class.

Sample code can be just like that:

	private void button1_Click(object sender, EventArgs e)
		{
	        CT_S851_Manager PrinterManager = new CT_S851_Manager();
		    if (PrinterManager.ConnectToPrinter("192.168.1.117") != 0)
		        return;

		    if (PrinterManager.SetUserEncoding(Encoding.GetEncoding(866)) != 0)
		        return;

		    //PrinterManager.SetInvertedPrintingMode(1);
		    PrinterManager.SetFontWeight(1);
		    PrinterManager.SetFontSize(1);
		    PrinterManager.PrintString("Тест\n");
		    PrinterManager.SetFontSize(2);
		    PrinterManager.PrintString("Тест\n");
		    PrinterManager.SetFontSize(3);
		    PrinterManager.PrintString("Тест\n");
		    PrinterManager.SetFontWeight(0);
		    PrinterManager.SetFontSize(1);
		    PrinterManager.PrintString("Тест\n");
		    PrinterManager.SetFontSize(2);
		    PrinterManager.PrintString("Тест\n");
		    PrinterManager.SetFontSize(3);
		    PrinterManager.PrintString("Тест");
		    PrinterManager.TicketCut();

		    PrinterManager.DisconnectFromPrinter();
		}