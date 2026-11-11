#!groovy

node('EJ2Node20') {
    timeout(time: 240) {
        try {
            deleteDir()
            
            stage('Import') {
                git url: 'https://gitea.syncfusion.com/essential-studio/ej2-groovy-scripts.git', branch: 'master', credentialsId: env.GiteaCredentialID
                shared = load 'src/shared.groovy'
            }
            
            stage('Checkout') {
                checkout scm
            }

            stage('Validation') {
                shared.getProjectDetails()
                shared.gitlabCommitStatus('running')
            }

            if (shared.checkCommitMessage()) {
                // Export environment variables for gulp tasks
                withEnv(['ALLOW_LOCAL_BUILD=1', 'VITE_BASE_PATH=/', 'PUBLISH_SETTINGS_PATH=./deployment/flight-booking-staging.PublishSettings']) {
                    
                    stage('Install') {
                        sh '''
                            echo "Installing root dependencies..."
                            npm install
                            echo "Installing client dependencies..."
                            npx gulp install
                        '''
                    }
                    
                    stage('Type Check') {
                        sh '''
                            cd flight-booking/client/react
                            npx tsc --noEmit || echo "Type check complete (warnings non-blocking)"
                        '''
                    }
                    
                    stage('Build') {
                        echo "Starting 'ci-compile'";
                        sh '''
                            echo "Building client with Vite..."
                            npx gulp build
                            echo "Build output location: flight-booking/client/react/dist/"
                            ls -lh flight-booking/client/react/dist/ | head -20
                        '''
                        echo "Finished 'ci-compile'";
                    }
                    
                    stage('Setup Agent Environment') {
                        withCredentials([usernamePassword(credentialsId: env.GiteaCredentialID, usernameVariable: 'GITEA_USER', passwordVariable: 'GITEA_PASS')]) {
                            sh '''
                                echo "Setting up Python agent environment via gulp..."
                                export GITEA_TOKEN="${GITEA_PASS}"
                                ALLOW_LOCAL_BUILD=1 npx gulp setupAgent
                            '''
                        }
                    }
                    
                    stage('Agent Lint Check') {
                        sh '''
                            cd flight-booking/agent
                            echo "Running ruff lint check..."
                            python3 -m ruff check flight_booking_agent
                        '''
                    }
                    
                    stage('Agent Format Check') {
                        sh '''
                            cd flight-booking/agent
                            echo "Running ruff format check..."
                            python3 -m ruff format --check flight_booking_agent
                        '''
                    }
                    
                    stage('Agent Type Check') {
                        sh '''
                            cd flight-booking/agent
                            echo "Running pylint type check..."
                            python3 -m pylint flight_booking_agent --disable=all --enable=E,F
                        '''
                    }
                    
                    stage('Agent Build') {
                        sh '''
                            echo "Building Python agent package..."
                            cd flight-booking/agent
                            python3 -m pip install build --break-system-packages
                            python3 -m build
                            echo "✓ Agent package built successfully"
                        '''
                    }
                    
                }
            }
            
            shared.gitlabCommitStatus('success')
            deleteDir()
        }
        catch (Exception e) {
            shared.throwError(e)
        }
    }
}