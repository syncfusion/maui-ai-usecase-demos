using System.Collections.ObjectModel;

namespace AIFileSearchTreeView.Data;

/// <summary>
/// Hard-coded sample data used to populate the TreeView. Kept in a dedicated
/// data layer so the ViewModel does not mix presentation state with file
/// generation.
/// </summary>
public static class SalesFileRepository
{
    public static ObservableCollection<SalesFileNode> CreateSampleTree()
    {
        var root = new ObservableCollection<SalesFileNode>();

        root.Add(BuildDownloadsNode());
        root.Add(BuildDocumentsNode());
        root.Add(BuildSharedResourcesNode());
        root.Add(BuildArchiveNode());

        return root;
    }

    private static SalesFileNode BuildDownloadsNode()
    {
        var node = new SalesFileNode
        {
            ItemName = "Downloads",
            ImageIcon = "treeview_folder.png",
            SearchPath = "/Downloads",
            FileType = "Folder",
            Keywords = new[] { "downloads", "incoming", "bills", "statements", "invoices" }
        };

        // Files here use TOPIC-BASED names only — no month or year suffixes — so
        // search queries like 'wifi invoice' or 'wifi coverage report' resolve to
        // a single, unambiguous file rather than collapsing to whatever happened
        // to be downloaded last month. Recency is communicated via CreatedDate.
        node.SubFiles = new ObservableCollection<FileManager>
        {
            new SalesFileNode
            {
                ItemName = "WiFi_Invoice.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Downloads/WiFi_Invoice.pdf",
                FileType = "Invoice",
                Keywords = new[] { "wifi", "wireless", "network", "invoice", "billing" },
                CreatedDate = new DateTime(2026, 9, 15),
                ModifiedDate = new DateTime(2026, 9, 15),
                InvoiceAmount = 55.00m
            },
            new SalesFileNode
            {
                ItemName = "Wireless_Invoice.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Downloads/Wireless_Invoice.pdf",
                FileType = "Invoice",
                Keywords = new[] { "wireless", "wifi", "network", "invoice", "billing" },
                CreatedDate = new DateTime(2026, 9, 10),
                ModifiedDate = new DateTime(2026, 9, 10),
                InvoiceAmount = 59.50m
            },
            new SalesFileNode
            {
                ItemName = "Network_Invoice.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Downloads/Network_Invoice.pdf",
                FileType = "Invoice",
                Keywords = new[] { "network", "internet", "connectivity", "invoice", "billing" },
                CreatedDate = new DateTime(2026, 9, 5),
                ModifiedDate = new DateTime(2026, 9, 5),
                InvoiceAmount = 67.25m
            },
            new SalesFileNode
            {
                ItemName = "Broadband_Invoice.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Downloads/Broadband_Invoice.pdf",
                FileType = "Invoice",
                Keywords = new[] { "broadband", "network", "internet", "wifi", "connectivity", "invoice", "billing" },
                CreatedDate = new DateTime(2026, 8, 28),
                ModifiedDate = new DateTime(2026, 8, 28),
                InvoiceAmount = 72.00m
            },
            new SalesFileNode
            {
                ItemName = "Connectivity_Invoice.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Downloads/Connectivity_Invoice.pdf",
                FileType = "Invoice",
                Keywords = new[] { "connectivity", "network", "internet", "invoice", "billing" },
                CreatedDate = new DateTime(2026, 8, 20),
                ModifiedDate = new DateTime(2026, 8, 20),
                InvoiceAmount = 64.99m
            },
            new SalesFileNode
            {
                ItemName = "Internet_Statement.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Downloads/Internet_Statement.pdf",
                FileType = "Invoice",
                Keywords = new[] { "internet", "network", "connectivity", "statement", "billing" },
                CreatedDate = new DateTime(2026, 8, 12),
                ModifiedDate = new DateTime(2026, 8, 12),
                InvoiceAmount = 58.75m
            },
            new SalesFileNode
            {
                ItemName = "Broadband_Summary.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Downloads/Broadband_Summary.pdf",
                FileType = "Summary",
                Keywords = new[] { "broadband", "network", "internet", "wifi", "summary" },
                CreatedDate = new DateTime(2026, 8, 10),
                ModifiedDate = new DateTime(2026, 8, 10)
            },
            new SalesFileNode
            {
                ItemName = "Connection_Renewal_Record.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Downloads/Connection_Renewal_Record.pdf",
                FileType = "Contract",
                Keywords = new[] { "connection", "renewal", "network", "internet", "record", "contract" },
                CreatedDate = new DateTime(2026, 8, 12),
                ModifiedDate = new DateTime(2026, 8, 12)
            }
        };

        return node;
    }

    private static SalesFileNode BuildDocumentsNode()
    {
        var node = new SalesFileNode
        {
            ItemName = "Documents",
            ImageIcon = "treeview_folder.png",
            SearchPath = "/Documents",
            FileType = "Folder",
            Keywords = new[] { "documents", "files", "records", "project" }
        };

        node.SubFiles = new ObservableCollection<FileManager>
        {
            new SalesFileNode
            {
                ItemName = "Project_Status_Report.docx",
                ImageIcon = "treeview_word.png",
                SearchPath = "/Documents/Project_Status_Report.docx",
                FileType = "Report",
                Keywords = new[] { "project", "status", "report", "progress" },
                CreatedDate = new DateTime(2026, 1, 15),
                ModifiedDate = new DateTime(2026, 1, 15)
            },
            new SalesFileNode
            {
                ItemName = "Network_Design.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Documents/Network_Design.pdf",
                FileType = "Document",
                Keywords = new[] { "network", "design", "topology", "architecture", "internet", "wifi" },
                CreatedDate = new DateTime(2026, 2, 2),
                ModifiedDate = new DateTime(2026, 2, 2)
            },
            new SalesFileNode
            {
                ItemName = "Meeting_Notes.docx",
                ImageIcon = "treeview_word.png",
                SearchPath = "/Documents/Meeting_Notes.docx",
                FileType = "Document",
                Keywords = new[] { "meeting", "notes", "minutes", "discussion" },
                CreatedDate = new DateTime(2026, 2, 20),
                ModifiedDate = new DateTime(2026, 2, 20)
            },
            new SalesFileNode
            {
                ItemName = "User_Guide.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Documents/User_Guide.pdf",
                FileType = "Document",
                Keywords = new[] { "user", "guide", "manual", "documentation" },
                CreatedDate = new DateTime(2026, 3, 10),
                ModifiedDate = new DateTime(2026, 3, 10)
            },
            new SalesFileNode
            {
                ItemName = "Budget_Plan.xlsx",
                ImageIcon = "treeview_excel.png",
                SearchPath = "/Documents/Budget_Plan.xlsx",
                FileType = "Document",
                Keywords = new[] { "budget", "plan", "finance", "expense" },
                CreatedDate = new DateTime(2026, 3, 25),
                ModifiedDate = new DateTime(2026, 3, 25)
            },
            new SalesFileNode
            {
                ItemName = "Training_Material.pptx",
                ImageIcon = "treeview_ppt.png",
                SearchPath = "/Documents/Training_Material.pptx",
                FileType = "Document",
                Keywords = new[] { "training", "material", "presentation", "learning" },
                CreatedDate = new DateTime(2026, 4, 8),
                ModifiedDate = new DateTime(2026, 4, 8)
            },
            new SalesFileNode
            {
                ItemName = "Requirement_Document.docx",
                ImageIcon = "treeview_word.png",
                SearchPath = "/Documents/Requirement_Document.docx",
                FileType = "Document",
                Keywords = new[] { "requirement", "document", "specification", "scope" },
                CreatedDate = new DateTime(2026, 4, 22),
                ModifiedDate = new DateTime(2026, 4, 22)
            },
            new SalesFileNode
            {
                ItemName = "Architecture_Diagram.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Documents/Architecture_Diagram.pdf",
                FileType = "Document",
                Keywords = new[] { "architecture", "diagram", "design", "system" },
                CreatedDate = new DateTime(2026, 5, 12),
                ModifiedDate = new DateTime(2026, 5, 12)
            },
            new SalesFileNode
            {
                ItemName = "Policy_Document.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Documents/Policy_Document.pdf",
                FileType = "Document",
                Keywords = new[] { "policy", "document", "compliance", "rules" },
                CreatedDate = new DateTime(2026, 6, 18),
                ModifiedDate = new DateTime(2026, 6, 18)
            },
            new SalesFileNode
            {
                ItemName = "Inventory_List.xlsx",
                ImageIcon = "treeview_excel.png",
                SearchPath = "/Documents/Inventory_List.xlsx",
                FileType = "Document",
                Keywords = new[] { "inventory", "list", "assets", "tracking" },
                CreatedDate = new DateTime(2026, 7, 1),
                ModifiedDate = new DateTime(2026, 7, 1)
            }
        };

        return node;
    }

    private static SalesFileNode BuildSharedResourcesNode()
    {
        var node = new SalesFileNode
        {
            ItemName = "Shared Resources",
            ImageIcon = "treeview_folder.png",
            SearchPath = "/Shared Resources",
            FileType = "Folder",
            Keywords = new[] { "shared", "team", "collaboration", "common" }
        };

        node.SubFiles = new ObservableCollection<FileManager>
        {
            new SalesFileNode
            {
                ItemName = "Coverage_Test_Report.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Shared Resources/Coverage_Test_Report.pdf",
                FileType = "Report",
                Keywords = new[] { "coverage", "test", "report", "network", "wifi", "wireless" },
                CreatedDate = new DateTime(2026, 1, 20),
                ModifiedDate = new DateTime(2026, 1, 20)
            },
            new SalesFileNode
            {
                ItemName = "Infrastructure_Notes.docx",
                ImageIcon = "treeview_word.png",
                SearchPath = "/Shared Resources/Infrastructure_Notes.docx",
                FileType = "Document",
                Keywords = new[] { "infrastructure", "notes", "network", "system" },
                CreatedDate = new DateTime(2026, 2, 12),
                ModifiedDate = new DateTime(2026, 2, 12)
            },
            new SalesFileNode
            {
                ItemName = "Network_Upgrade_Plan.xlsx",
                ImageIcon = "treeview_excel.png",
                SearchPath = "/Shared Resources/Network_Upgrade_Plan.xlsx",
                FileType = "Document",
                Keywords = new[] { "network", "upgrade", "plan", "internet", "wifi", "connectivity" },
                CreatedDate = new DateTime(2026, 3, 5),
                ModifiedDate = new DateTime(2026, 3, 5)
            },
            new SalesFileNode
            {
                ItemName = "Access_Point_List.xlsx",
                ImageIcon = "treeview_excel.png",
                SearchPath = "/Shared Resources/Access_Point_List.xlsx",
                FileType = "Document",
                Keywords = new[] { "access point", "list", "wifi", "wireless", "network" },
                CreatedDate = new DateTime(2026, 3, 28),
                ModifiedDate = new DateTime(2026, 3, 28)
            },
            new SalesFileNode
            {
                ItemName = "Firewall_Settings.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Shared Resources/Firewall_Settings.pdf",
                FileType = "Document",
                Keywords = new[] { "firewall", "settings", "security", "network", "configuration" },
                CreatedDate = new DateTime(2026, 4, 14),
                ModifiedDate = new DateTime(2026, 4, 14)
            },
            new SalesFileNode
            {
                ItemName = "Implementation_Plan.docx",
                ImageIcon = "treeview_word.png",
                SearchPath = "/Shared Resources/Implementation_Plan.docx",
                FileType = "Document",
                Keywords = new[] { "implementation", "plan", "rollout", "strategy" },
                CreatedDate = new DateTime(2026, 5, 2),
                ModifiedDate = new DateTime(2026, 5, 2)
            },
            new SalesFileNode
            {
                ItemName = "Wireless_Survey_Report.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Shared Resources/Wireless_Survey_Report.pdf",
                FileType = "Report",
                Keywords = new[] { "wireless", "wifi", "network", "survey", "report" },
                CreatedDate = new DateTime(2026, 5, 20),
                ModifiedDate = new DateTime(2026, 5, 20)
            },
            new SalesFileNode
            {
                ItemName = "Operations_Guide.docx",
                ImageIcon = "treeview_word.png",
                SearchPath = "/Shared Resources/Operations_Guide.docx",
                FileType = "Document",
                Keywords = new[] { "operations", "guide", "process", "procedure" },
                CreatedDate = new DateTime(2026, 6, 8),
                ModifiedDate = new DateTime(2026, 6, 8)
            },
            new SalesFileNode
            {
                ItemName = "Performance_Report.xlsx",
                ImageIcon = "treeview_excel.png",
                SearchPath = "/Shared Resources/Performance_Report.xlsx",
                FileType = "Report",
                Keywords = new[] { "performance", "report", "metrics", "kpi" },
                CreatedDate = new DateTime(2026, 7, 3),
                ModifiedDate = new DateTime(2026, 7, 3)
            },
            new SalesFileNode
            {
                ItemName = "Technical_Document.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Shared Resources/Technical_Document.pdf",
                FileType = "Document",
                Keywords = new[] { "technical", "document", "specification", "engineering" },
                CreatedDate = new DateTime(2026, 7, 25),
                ModifiedDate = new DateTime(2026, 7, 25)
            }
        };

        return node;
    }

    private static SalesFileNode BuildArchiveNode()
    {
        var node = new SalesFileNode
        {
            ItemName = "Archive",
            ImageIcon = "treeview_folder.png",
            SearchPath = "/Archive",
            FileType = "Folder",
            Keywords = new[] { "archive", "backup", "old", "legacy" }
        };

        node.SubFiles = new ObservableCollection<FileManager>
        {
            new SalesFileNode
            {
                ItemName = "Random_Notes.txt",
                ImageIcon = "treeview_text.png",
                SearchPath = "/Archive/Random_Notes.txt",
                FileType = "Document",
                Keywords = new[] { "random", "notes", "text", "scratch" },
                CreatedDate = new DateTime(2026, 1, 10),
                ModifiedDate = new DateTime(2026, 1, 10)
            },
            new SalesFileNode
            {
                ItemName = "Old_Report.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Archive/Old_Report.pdf",
                FileType = "Report",
                Keywords = new[] { "old", "report", "archive", "legacy" },
                CreatedDate = new DateTime(2025, 12, 5),
                ModifiedDate = new DateTime(2025, 12, 5)
            },
            new SalesFileNode
            {
                ItemName = "Draft_Document.docx",
                ImageIcon = "treeview_word.png",
                SearchPath = "/Archive/Draft_Document.docx",
                FileType = "Document",
                Keywords = new[] { "draft", "document", "work in progress", "wip" },
                CreatedDate = new DateTime(2026, 3, 18),
                ModifiedDate = new DateTime(2026, 3, 18)
            },
            new SalesFileNode
            {
                ItemName = "Temp_File.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Archive/Temp_File.pdf",
                FileType = "Document",
                Keywords = new[] { "temp", "temporary", "file" },
                CreatedDate = new DateTime(2026, 4, 30),
                ModifiedDate = new DateTime(2026, 4, 30)
            },
            new SalesFileNode
            {
                ItemName = "Equipment_Details.xlsx",
                ImageIcon = "treeview_excel.png",
                SearchPath = "/Archive/Equipment_Details.xlsx",
                FileType = "Document",
                Keywords = new[] { "equipment", "details", "hardware", "specs" },
                CreatedDate = new DateTime(2026, 5, 22),
                ModifiedDate = new DateTime(2026, 5, 22)
            },
            new SalesFileNode
            {
                ItemName = "Reference_Document.pdf",
                ImageIcon = "treeview_pdf.png",
                SearchPath = "/Archive/Reference_Document.pdf",
                FileType = "Document",
                Keywords = new[] { "reference", "document", "resource" },
                CreatedDate = new DateTime(2026, 6, 14),
                ModifiedDate = new DateTime(2026, 6, 14)
            },
            new SalesFileNode
            {
                ItemName = "Test_Data.csv",
                ImageIcon = "treeview_csv.png",
                SearchPath = "/Archive/Test_Data.csv",
                FileType = "Document",
                Keywords = new[] { "test", "data", "sample", "csv" },
                CreatedDate = new DateTime(2026, 7, 9),
                ModifiedDate = new DateTime(2026, 7, 9)
            },
            new SalesFileNode
            {
                ItemName = "General_Notes.docx",
                ImageIcon = "treeview_word.png",
                SearchPath = "/Archive/General_Notes.docx",
                FileType = "Document",
                Keywords = new[] { "general", "notes", "miscellaneous" },
                CreatedDate = new DateTime(2026, 8, 1),
                ModifiedDate = new DateTime(2026, 8, 1)
            }
        };

        return node;
    }
}