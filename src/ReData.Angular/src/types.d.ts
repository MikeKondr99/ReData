export interface DataSource {
  id: string;
  type: DataSourceType;
  name: string;
  description: string | null;
}

export type DataSourceType = 'PostgreSql' | 'Csv';
