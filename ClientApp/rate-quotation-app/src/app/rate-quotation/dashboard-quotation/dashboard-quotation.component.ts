import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

interface Quotation {
  quoteNo: string;
  companyName: string;
  styleNo: string;
  embDesign: string;
  stitches: number;
  cost: number;
  paymentTerms: string;
  status: 'Approved' | 'Draft' | 'Sent' | 'Rejected' | 'Expired';
  date: Date;
}

@Component({
  selector: 'app-dashboard-quotation',
  templateUrl: './dashboard-quotation.component.html',
  styleUrls: ['./dashboard-quotation.component.scss']
})
export class DashboardQuotationComponent implements OnInit {
  displayedColumns: string[] = ['quoteNo', 'companyName', 'styleNo', 'embDesign', 'stitches', 'cost', 'paymentTerms', 'status', 'date', 'actions'];
  dataSource: Quotation[] = [];

  constructor(private router: Router) { }

  ngOnInit(): void {
    // Hardcoded sample data
    this.dataSource = [
      { quoteNo: 'QT-2023-001', companyName: 'Acme Corp', styleNo: 'ST-001', embDesign: 'Floral', stitches: 15000, cost: 2.5, paymentTerms: 'Net 30', status: 'Approved', date: new Date('2023-10-01') },
      { quoteNo: 'QT-2023-002', companyName: 'Globex', styleNo: 'ST-002', embDesign: 'Geometric', stitches: 8000, cost: 1.8, paymentTerms: 'Net 15', status: 'Draft', date: new Date('2023-10-05') },
      { quoteNo: 'QT-2023-003', companyName: 'Initech', styleNo: 'ST-003', embDesign: 'Logo', stitches: 5000, cost: 1.2, paymentTerms: 'Due on Receipt', status: 'Sent', date: new Date('2023-10-10') },
      { quoteNo: 'QT-2023-004', companyName: 'Umbrella Corp', styleNo: 'ST-004', embDesign: 'Abstract', stitches: 22000, cost: 3.0, paymentTerms: 'Net 60', status: 'Rejected', date: new Date('2023-10-12') },
      { quoteNo: 'QT-2023-005', companyName: 'Stark Ind', styleNo: 'ST-005', embDesign: 'Badge', stitches: 12000, cost: 2.0, paymentTerms: 'Net 30', status: 'Expired', date: new Date('2023-09-01') },
    ];
  }

  navigateToCreate() {
    this.router.navigate(['/rate-quotation/create']);
  }
}
