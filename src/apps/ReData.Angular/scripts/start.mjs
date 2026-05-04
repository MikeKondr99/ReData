import { spawn } from "node:child_process";

const port = process.env.PORT || "4200";
const args = [
  "serve",
  "--host",
  "0.0.0.0",
  "--port",
  port,
  "--proxy-config",
  "proxy.conf.cjs"
];

const child = spawn("ng", args, {
  cwd: process.cwd(),
  env: process.env,
  stdio: "inherit",
  shell: true
});

child.on("exit", (code) => {
  process.exit(code ?? 1);
});
