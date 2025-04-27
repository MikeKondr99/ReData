import {Data} from '@angular/router';

export type TransformationType = 'where' | 'orderBy';

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

export type Transformation = WhereTransformation | OrderByTransformation | SelectTransformation;

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



export type FunctionKind = 'Default' | 'Method' | 'Binary' | 'Unary';
export type DataType =
  'Unknown' |
  'Null' |
  'Number' |
  'Integer' |
  'Text' |
  'Bool' |
  'DateTime';
