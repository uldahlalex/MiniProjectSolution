import {BrowserModule} from '@angular/platform-browser';

import {enableProdMode, ErrorHandler, NgModule} from "@angular/core";

import {ToastModule} from "primeng/toast";
import {MessageModule} from "primeng/message";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {CommonModule} from "@angular/common";
import {platformBrowserDynamic} from "@angular/platform-browser-dynamic";
import {MessageService} from "primeng/api";import {AppComponent} from "./app/app.component";
import {ImageDetectionComponent} from "./app/image.detection.component";


@NgModule({
  imports: [
    BrowserModule,
    CommonModule,
    ToastModule,
    MessageModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    FormsModule,

  ],
  declarations: [
    ImageDetectionComponent,
    AppComponent
  ],
  providers: [MessageService, ],
  bootstrap: [AppComponent]
})
export class AppModule {
}


platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.log(err));
