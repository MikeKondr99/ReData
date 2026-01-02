export * from './datasets.service';
import { DatasetsService } from './datasets.service';
export * from './datasets.serviceInterface';
export * from './default.service';
import { DefaultService } from './default.service';
export * from './default.serviceInterface';
export const APIS = [DatasetsService, DefaultService];
