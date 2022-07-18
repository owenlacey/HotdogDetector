import { Component, ElementRef, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'hotdog-detector-web';
  loading = false;
  prediction: HotDogPredictionModel | null = null;
  imageSrc: string | null = null;
  @ViewChild('fileInput')fileInput: ElementRef<HTMLInputElement> = null!;

  constructor(private http: HttpClient) { }

  onFileChange(ev: Event) {
    ev.preventDefault();
    const target = (ev.target as any);
    if(target.files?.length != null) {
      this.showPreview(target);
      this.getPrediction();
    }
  }

  private showPreview(target: any) {
    const reader = new FileReader();
    const [file] = target.files;
    reader.onload = () => {  
      this.imageSrc = reader.result as string;  
    };  
    reader.readAsDataURL(file);  
  }

  private getPrediction() {    
    const input = this.fileInput.nativeElement as any;
    const file = input.files[0];
    this.loading = true;
    var formData = new FormData();
    formData.append("file",  file, file.name);
    this.http.post<HotDogPrediction>(
      `https://localhost:51457/predict`, 
      formData,
      {
        headers:  {
          'Accept': 'application/json'
        }
      })
    .subscribe((res: HotDogPrediction) => {
      this.prediction = {
        isHotdog: res.predictedLabel === 'Hotdog',
        isHotdogPercentage: res.score[0],
        isNotHotdogPercentage: res.score[1]
      }
    })
  }
}

export interface HotDogPrediction {
  predictedLabel: string;
  score: number[];
}

export interface HotDogPredictionModel {
  isHotdog: boolean;
  isHotdogPercentage: number;
  isNotHotdogPercentage: number;
}
