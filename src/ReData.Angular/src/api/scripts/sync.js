const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

console.log('🚀 Generating Angular API client...');

// Configuration
const CONFIG = {
  backendUrl: 'http://localhost:5223',
  apiDir: path.join(__dirname, './'),
  outputDir: path.join(__dirname, '../')
};

async function generateApi() {
  try {
    console.log('📥 Fetching OpenAPI spec from backend...');

    // 1. Fetch OpenAPI JSON from backend
    const response = await fetch(`${CONFIG.backendUrl}/openapi/v1.json`);
    if (!response.ok) {
      throw new Error(`Failed to fetch: ${response.status} ${response.statusText}`);
    }

    const openapiJson = await response.text();

    // 2. Save to api directory
    const specPath = path.join(CONFIG.apiDir, 'openapi.json');
    fs.writeFileSync(specPath, openapiJson);
    console.log(`✅ OpenAPI spec saved to: ${specPath}`);

    // 3. Run Docker Compose to generate client
    console.log('🐳 Running OpenAPI Generator...');
    const dockerComposeFile = path.join(CONFIG.apiDir, 'docker-compose.yaml');

    execSync(`docker-compose -f "${dockerComposeFile}" up --build --force-recreate`, {
      cwd: CONFIG.apiDir,
      stdio: 'inherit'
    });

    console.log('✨ Angular API client generated successfully!');

  } catch (error) {
    console.error('❌ Error generating API:', error.message);
    process.exit(1);
  }
}

// Check if backend is running first
async function checkBackend() {
  try {
    console.log(`🔍 Checking backend at ${CONFIG.backendUrl}...`);
    const response = await fetch(CONFIG.backendUrl, { timeout: 2000 });
    return response.ok;
  } catch {
    return false;
  }
}

async function main() {
  const isBackendRunning = await checkBackend();

  if (!isBackendRunning) {
    console.log('⚠️  Backend not detected. Please ensure:');
    console.log(`   1. Backend is running at ${CONFIG.backendUrl}`);
    console.log('   2. You have FastEndpoints.Swagger configured');
    console.log('\nTo start backend:');
    console.log('   cd ../ReData.DemoApp');
    console.log('   dotnet run --environment Development');
    process.exit(1);
  }

  await generateApi();
}

main();
