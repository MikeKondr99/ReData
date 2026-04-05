import {
	getAllFunctions,
	type getAllFunctionsResponse
} from '$lib/api/generated/default/default';
import type { FunctionResponse } from '$lib/api/generated/model';

export type { getAllFunctionsResponse, FunctionResponse };

let functionCache: Promise<getAllFunctionsResponse> | undefined = undefined;

export async function getFunctions(): Promise<getAllFunctionsResponse> {
	if (functionCache === undefined) {
		functionCache = getAllFunctions().catch((e) => {
			functionCache = undefined;
			throw e;
		});
	}
	return functionCache;
}
