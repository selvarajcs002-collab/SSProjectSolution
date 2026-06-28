import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-create-quotation',
  templateUrl: './create-quotation.component.html',
  styleUrls: ['./create-quotation.component.scss']
})
export class CreateQuotationComponent {
  quotationForm: FormGroup;
  imagePreview: string | ArrayBuffer | null = null;

  constructor(private fb: FormBuilder) {
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
    console.log('Generating PDF...');
  }

  submitQuotation() {
    if (this.quotationForm.valid) {
      console.log('Quotation submitted', this.quotationForm.value);
    } else {
      this.quotationForm.markAllAsTouched();
    }
  }
}
