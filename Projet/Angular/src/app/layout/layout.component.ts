import { Component, OnInit, ViewChild, AfterContentChecked, HostListener } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { MatToolbar } from '@angular/material/toolbar';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent implements OnInit, AfterContentChecked {

  opened = true;
  @ViewChild('sidenav', { static: true }) sidenav: MatSidenav;
  @ViewChild('toolbar', { static: true }) toolbar: MatToolbar;

  constructor() { }

  ngAfterContentChecked(): void {
    this.resizeSidebar(window.innerWidth);
  }

  ngOnInit(): void {
    this.resizeSidebar(window.innerWidth);
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.resizeSidebar(event.target.innerWidth);
  }

  isBiggerScreen() {
    const width = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
    return width < 768;
  }

  resizeSidebar(windowSize){
    this.sidenav.fixedTopGap = this.getHeightToolbar();
    this.opened = windowSize >= 768;
  }

  getHeightToolbar(){
    return this.toolbar._elementRef.nativeElement.clientHeight;
  }

  

}
