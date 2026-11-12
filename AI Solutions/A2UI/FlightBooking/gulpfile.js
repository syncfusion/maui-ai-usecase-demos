'use strict';

var shelljs = global.shelljs = global.shelljs || require('shelljs');
var path = global.path = global.path || require('path');

// Sub-project that owns the build. install/build operate from here so
// they pick up the correct package.json and node_modules without
// callers having to `cd` first.
var CLIENT_DIR = path.resolve(__dirname, 'flight-booking', 'client', 'react');

// `npm install` and `npm run build` are heavy and mutate global state
// (node_modules, lockfile, on-disk `.npmrc`). They are intended for CI.
// To stop an accidental `npx gulp build` on a developer laptop from
// reinstalling and rebuilding the whole client, the tasks refuse to
// run unless ALLOW_LOCAL_BUILD is exported in the current shell.
// Jenkins exports it explicitly; developers can opt in per-command.
function assertAllowedInThisShell() {
  if (!process.env.ALLOW_LOCAL_BUILD) {
    throw new Error(
      'Refusing to run: this task installs/builds and is intended for CI.\n' +
      'Set ALLOW_LOCAL_BUILD=1 in your shell if you really want to run it locally.\n' +
      '  (Windows)   set ALLOW_LOCAL_BUILD=1 && npx gulp build\n' +
      '  (POSIX)     ALLOW_LOCAL_BUILD=1 npx gulp build'
    );
  }
}

// Runs `cmd` inside CLIENT_DIR. Throws if cmd exits non-zero.
function runInClient(label, cmd) {
  console.log('\u203a ' + label + ': cd ' + CLIENT_DIR + ' && ' + cmd);
  var result = shelljs.exec(cmd, { cwd: CLIENT_DIR, silent: false });
  if (result.code !== 0) {
    throw new Error(label + ' failed (exit ' + result.code + ')');
  }
}

/**
 * Install dependencies for the flight-booking client sub-project.
 *
 * Gated by ALLOW_LOCAL_BUILD so it cannot be triggered accidentally
 * on a developer laptop. Jenkins sets ALLOW_LOCAL_BUILD=1.
 *
 * Runs: cd flight-booking/client/react && npm install
 *
 * Usage:
 *   ALLOW_LOCAL_BUILD=1 npx gulp install
 */
function install(done) {
  try {
    assertAllowedInThisShell();
    console.log('Installing client dependencies...');
    runInClient('npm install', 'npm install');
    console.log('\u2713 npm install completed\n');
    done();
  } catch (err) {
    done(err);
  }
}

/**
 * Build the flight-booking client sub-project.
 *
 * Gated by ALLOW_LOCAL_BUILD so it cannot be triggered accidentally
 * on a developer laptop. Jenkins sets ALLOW_LOCAL_BUILD=1.
 *
 * Runs: cd flight-booking/client/react && npm run build
 *
 * Usage:
 *   ALLOW_LOCAL_BUILD=1 npx gulp build
 */
function build(done) {
  try {
    assertAllowedInThisShell();
    console.log('Building client...');
    runInClient('npm run build', 'npm run build');
    console.log('\u2713 npm run build completed\n');
    done();
  } catch (err) {
    done(err);
  }
}

/**
 * Setup Python agent environment.
 *
 * Gated by ALLOW_LOCAL_BUILD so it cannot be triggered accidentally
 * on a developer laptop. Jenkins sets ALLOW_LOCAL_BUILD=1.
 *
 * Installs:
 *   - pip, setuptools, wheel
 *   - syncfusion-a2ui-agent ADK from GitHub (with GITEA_TOKEN if private)
 *   - flight-booking-agent package in editable mode
 *   - pylint for type checking
 *   - ruff for linting and formatting
 *
 * Usage:
 *   ALLOW_LOCAL_BUILD=1 GITEA_TOKEN=<token> npx gulp setupAgent
 */
function setupAgent(done) {
  try {
    assertAllowedInThisShell();
    
    var giteaToken = process.env.GITEA_TOKEN || '';
    console.log('Setting up Python agent environment...');
    
    var agentDir = path.resolve(__dirname, 'flight-booking', 'agent');
    var isWindows = process.platform === 'win32';
    
    // Check Python version
    var versionResult = shelljs.exec('python3 --version', { silent: true });
    console.log('\u203a Python version: ' + versionResult.stdout.trim());
    
    // Install pip with sudo (Linux/Jenkins only)
    if (!isWindows) {
      console.log('\u203a Installing pip with sudo...');
      var pipInstall = shelljs.exec('sudo apt-get update && sudo apt-get install -y python3-pip', { silent: false });
      if (pipInstall.code !== 0) {
        throw new Error('Failed to install python3-pip');
      }
    } else {
      console.log('\u203a Skipping apt-get (Windows detected)');
    }
    
    // Upgrade pip and build tools
    console.log('\u203a Upgrading pip and installing build tools...');
    var pipCmd = isWindows 
      ? 'python3 -m pip install --upgrade pip setuptools wheel' 
      : 'python3 -m pip install --upgrade pip setuptools wheel --break-system-packages';
    var upgradeResult = shelljs.exec(pipCmd, { silent: false });
    if (upgradeResult.code !== 0) {
      throw new Error('Failed to upgrade pip and build tools');
    }
    
    // Install flight-booking-agent package in editable mode (this handles all dependencies)
    // The pyproject.toml contains the syncfusion-a2ui-agent dependency
    console.log('\u203a Installing flight-booking-agent package with dependencies...');
    var pipFlag = isWindows ? '' : ' --break-system-packages';
    var installResult = shelljs.exec('python3 -m pip install -e .[dev]' + pipFlag, { 
      cwd: agentDir,
      silent: false
    });
    if (installResult.code !== 0) {
      throw new Error('Failed to install flight-booking-agent package');
    }
    
    // Install additional linting/analysis tools for type checking
    console.log('\u203a Installing pylint for type checking...');
    var pylintCmd = isWindows 
      ? 'python3 -m pip install pylint --quiet' 
      : 'python3 -m pip install pylint --quiet --break-system-packages';
    var pylintResult = shelljs.exec(pylintCmd, { silent: false });
    if (pylintResult.code !== 0) {
      throw new Error('Failed to install pylint');
    }
    
    // Install ruff for linting and formatting
    console.log('\u203a Installing ruff for linting and formatting...');
    var ruffCmd = isWindows 
      ? 'python3 -m pip install ruff --quiet' 
      : 'python3 -m pip install ruff --quiet --break-system-packages';
    var ruffResult = shelljs.exec(ruffCmd, { silent: false });
    if (ruffResult.code !== 0) {
      throw new Error('Failed to install ruff');
    }
    
    console.log('\u2713 Agent environment setup completed\n');
    done();
  } catch (err) {
    done(err);
  }
}

module.exports = {
  install: install,
  build: build,
  setupAgent: setupAgent
};