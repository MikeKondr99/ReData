import { configureStore } from '@reduxjs/toolkit';
import { dataSourceApi } from './services/dataSourceApi';

export const store = configureStore({
  reducer: {
    [dataSourceApi.reducerPath]: dataSourceApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat([dataSourceApi.middleware]),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
