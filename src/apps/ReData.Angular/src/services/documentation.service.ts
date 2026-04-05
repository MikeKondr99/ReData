import {inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {NzTreeNode, NzTreeNodeOptions} from 'ng-zorro-antd/tree';

export interface DocNode {
  name: string;
  path?: string;
  type: 'file' | 'folder';
  children?: DocNode[];
}

@Injectable({
  providedIn: 'root'
})
export class DocumentationService {

  http = inject(HttpClient);


  readonly nodes: NzTreeNodeOptions[] = [
    {
      title: 'Введение',
      key: 'Введение',
      isLeaf: true,
    },
    {
      title: 'Синтаксис',
      key: 'Синтаксис',
      isLeaf: false,
      expanded: true,
      selectable: false,
      children: [
        {
          title: 'Имена полей',
          key: 'Синтаксис/Имена полей',
          isLeaf: true,
        },
        {
          title: 'Литералы',
          key: 'Синтаксис/Литералы',
          isLeaf: true,
        },
        {
          title: 'Базовые операции',
          key: 'Синтаксис/Базовые операции',
          isLeaf: true,
        },
        {
          title: 'Функции',
          key: 'Синтаксис/Функции',
          isLeaf: true,
        },

      ]
    },
  ];

  getDocsStructure(): NzTreeNodeOptions[] {
    return this.nodes;
  }

  getMarkdownFile(path: string): Observable<string> {
    return this.http.get(`/assets/docs/${path}.md`, { responseType: 'text' });
  }
}
