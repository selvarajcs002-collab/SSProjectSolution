import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RateQuotationService } from '../rate-quotation.service';

@Component({
  selector: 'app-create-quotation',
  templateUrl: './create-quotation.component.html',
  styleUrls: ['./create-quotation.component.scss']
})
export class CreateQuotationComponent {
  quotationForm: FormGroup;
  imagePreview: string | ArrayBuffer | null = null;
  isGeneratingPdf = false;

  constructor(private fb: FormBuilder, private rateQuotationService: RateQuotationService) {
    this.quotationForm = this.fb.group({
      companyName: ['', Validators.required],
      styleNo: ['', Validators.required],
      embDesign: ['', Validators.required],
      noOfStitches: ['', [Validators.required, Validators.min(0)]],
      chenilleColors: ['', Validators.required],
      normalEmbColors: ['', Validators.required],
      embCost: ['', [Validators.required, Validators.min(0)]],
      paymentTerms: ['', Validators.required]
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        if (e.target && e.target.result) {
          this.imagePreview = e.target.result;
        }
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage() {
    this.imagePreview = null;
  }

  saveDraft() {
    console.log('Draft saved', this.quotationForm.value);
  }

  generatePdf() {
    this.isGeneratingPdf = true;
    console.log('Generating PDF...');

    // Hardcoded to quotationId: 2 as requested
    const savedQuotationId = 2; 

    this.rateQuotationService.downloadPdf(savedQuotationId).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `RateQuotation_${savedQuotationId}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        
        this.isGeneratingPdf = false;
        console.log('PDF Downloaded successfully!');
      },
      error: (error) => {
        console.error('Error generating PDF:', error);
        this.isGeneratingPdf = false;
      }
    });
  }

  submitQuotation() {
    if (this.quotationForm.valid) {
      console.log('Quotation submitted', this.quotationForm.value);
    } else {
      this.quotationForm.markAllAsTouched();
    }
  }
}

