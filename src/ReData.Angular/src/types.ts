import {Data} from '@angular/router';

import * as monaco from 'monaco-editor';
import * as MonacoModule from 'monaco-editor';

export type IEditor = typeof monaco.editor;
export type Monaco = typeof MonacoModule;

export type TransformationType = 'where' | 'orderBy' | 'select' | 'limit';

export type WhereTransformation = {
  $type: 'where',
  condition: string,
}

export type OrderByTransformation = {
  $type: 'orderBy';
  items: OrderByItem[];
};

export type SelectTransformation = {
  $type: 'select';
  items: SelectItem[];
};

export type LimitTransformation = {
  $type: 'limit';
  limit: number;
  offset: number;
};

export type Transformation = WhereTransformation | OrderByTransformation | SelectTransformation | LimitTransformation;

// Type guards
export function isWhereTransformation(t: Transformation): t is WhereTransformation {
  return t.$type === 'where';
}

export function isOrderByTransformation(t: Transformation): t is OrderByTransformation {
  return t.$type === 'orderBy';
}

export function isSelectTransformation(t: Transformation): t is SelectTransformation {
  return t.$type === 'select';
}

export function isLimitTransformation(t: Transformation): t is LimitTransformation {
  return t.$type === 'limit';
}

export interface Field {
  alias: string;
  type: 'Number' | 'Text' | 'Integer' | 'Boolean' | 'DateTime' | 'Unknown';
  canBeNull: boolean;
}

export interface ApiResponse {
  fields: Field[];
  total: number;
  query: string[];
  data: any[];
}

export type OrderByItem = {
  expression: string;
  descending: boolean;
};

export type SelectItem = {
  field: string;
  expression: string;
};

export interface FunctionViewModel
{
  name: string;
  doc: string;
  arguments: FunctionArgument[];
  returnType: ReturnType;
  kind: FunctionKind;
}

export interface FunctionArgument
{
  name: string;
  type: FunctionArgumentType
  propagateNull: boolean
}

export interface FunctionArgumentType
{
  dataType: DataType;
  canBeNull: boolean;
}

export interface ReturnType
{
  dataType: DataType
  canBeNull: boolean
  aggregated: boolean,
}

export interface ExprError {
  span: ExprSpan
  message: string;

}

export interface ExprSpan {
  line: number;
  column: number;
  length: number;
}



export type FunctionKind = 'Default' | 'Method' | 'Binary' | 'Unary';
export type DataType =
  'Unknown' |
  'Null' |
  'Number' |
  'Integer' |
  'Text' |
  'Bool' |
  'DateTime';
