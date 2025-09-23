




export function groupBy<T>(array: T[], f: (x: T) => string): Record<string, T[]> {
  return array.reduce(function (r: Record<string, T[]>, a) {
    let key = f(a);
    r[key] = r[key] || [];
    r[key].push(a);
    return r;
  }, {});
}
