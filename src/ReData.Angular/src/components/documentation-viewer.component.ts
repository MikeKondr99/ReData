import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MarkdownComponent } from 'ngx-markdown';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import {NzFormatEmitEvent, NzTreeModule, NzTreeNodeOptions} from 'ng-zorro-antd/tree';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { HttpClient } from '@angular/common/http';
import { catchError, of } from 'rxjs';
import {DocumentationService} from '../services/documentation.service';
import {ActivatedRoute, Router} from '@angular/router';

@Component({
  selector: 'app-documentation-viewer',
  standalone: true,
  imports: [
    CommonModule,
    MarkdownComponent,
    NzLayoutModule,
    NzTreeModule,
    NzIconModule
  ],
  template: `
    <nz-layout class="documentation-layout">
      <nz-sider nzWidth="280px" nzTheme="light">
        <nz-tree
          [nzData]="docStructure"
          [nzExpandAll]="true"
          (nzClick)="onTreeNodeClick($event)"
          nzBlockNode>
        </nz-tree>
      </nz-sider>

      <nz-layout>
        <nz-content class="content">
          @if (markdownContent()) {
            <div class="prose prose-pre:text-black prose-pre:bg-gray-50 prose-code:before:">
              <markdown  [data]="markdownContent()" />
            </div>
          } @else {
            <div class="no-content">
              Select a documentation file from the sidebar
            </div>
          }
        </nz-content>
      </nz-layout>
    </nz-layout>
  `,
  styles: [`
    .documentation-layout {
      height: 100vh;
    }

    .prose :where(code):not(:where([class~="not-prose"], [class~="not-prose"] *))::before {
      content: "" !important;
    }

    .prose :where(code):not(:where([class~="not-prose"], [class~="not-prose"] *))::after {
      content: "" !important;
    }

    .content {
      padding: 24px;
      background: #fff;
      overflow-y: auto;
    }

    .markdown-container {
      max-width: 800px;
      margin: 0 auto;
    }

    .no-content {
      text-align: center;
      padding: 48px;
      color: #999;
    }
  `]
})
export class DocumentationViewerComponent {
  private http = inject(HttpClient);
  private docService = inject(DocumentationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  docStructure = this.docService.getDocsStructure();
  selectedDoc = signal<string>('test.md');
  markdownContent = signal<string>('');
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  constructor() {
    this.loadDocument(this.selectedDoc());
  }

  ngOnInit() {
    // Subscribe to route parameters to handle direct links and browser navigation
    this.route.paramMap.subscribe(params => {
      const docPath = params.get('path');
      if (docPath) {
        this.loadDocument(docPath);
      } else {
        // Load default document if no path specified
        const defaultDoc = this.findFirstDocument(this.docStructure);
        if (defaultDoc) {
          this.navigateToDocument(defaultDoc.key!);
        }
      }
    });
  }

  loadDocument(path: string) {
    this.isLoading.set(true);
    this.error.set(null);
    this.selectedDoc.set(path);

    this.http.get(`/assets/docs/${path}.md`, { responseType: 'text' })
      .pipe(
        catchError(err => {
          this.error.set(`Failed to load document: ${err.message}`);
          this.markdownContent.set('# Document Not Found\n\nThe requested documentation file could not be loaded.');
          return of('');
        })
      )
      .subscribe(content => {
        this.markdownContent.set(content);
        this.isLoading.set(false);
      });
  }

  onTreeNodeClick(event: NzFormatEmitEvent): void {
    const node = event.node;
    if (node && node.key && !node.isLeaf) {
      // It's a folder, toggle expand
      node.isExpanded = !node.isExpanded;
    } else if (node && node.key && node.isLeaf) {
      // It's a file, navigate to it
      this.navigateToDocument(node.key);
    }
  }

  private navigateToDocument(path: string): void {
    // Use Angular router to navigate, which will update URL and browser history
    this.router.navigate(['/docs', path]);
  }

  private updateBrowserHistory(path: string): void {
    // This ensures the back/forward buttons work properly
    const currentUrl = this.router.createUrlTree(['/docs', path]).toString();
    window.history.replaceState({ path }, '', currentUrl);
  }

  private findFirstDocument(nodes: NzTreeNodeOptions[]): NzTreeNodeOptions | null {
    for (const node of nodes) {
      if (node.isLeaf) {
        return node;
      }
      if (node.children) {
        const found = this.findFirstDocument(node.children);
        if (found) return found;
      }
    }
    return null;
  }

}
