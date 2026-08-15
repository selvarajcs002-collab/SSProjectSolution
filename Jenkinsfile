pipeline {

    agent any
    
    triggers {
        githubPush()
    }

    options {
        timestamps()
        disableConcurrentBuilds()
        skipDefaultCheckout(true)
        timeout(time: 20, unit: 'MINUTES')

        buildDiscarder(
            logRotator(
                numToKeepStr: '20',
                artifactNumToKeepStr: '10'
            )
        )
    }

    environment {

        APP_NAME = "SSManagement-API-DEV"

        DEPLOY_PATH = "/var/www/SSManagement/DEV/API"

        PUBLISH_PATH = "${WORKSPACE}/publish"

        SERVICE_NAME = "ssmanagement-dev-api"

        DOTNET_ENVIRONMENT = "Development"

        API_PORT = "5000"
    }

    stages {

        stage('Pre-Check') {

            steps {

                sh '''
                    /bin/bash <<'SCRIPT'
                    set -Eeuo pipefail

                    echo "=========================================="
                    echo " SSManagement API DEV Deployment"
                    echo "=========================================="

                    echo ""
                    echo "Checking .NET SDK..."

                    if ! command -v dotnet >/dev/null 2>&1; then
                        echo "ERROR: dotnet command not found."
                        exit 1
                    fi

                    DOTNET_VERSION=$(dotnet --version)

                    echo "Installed .NET SDK: $DOTNET_VERSION"

                    case "$DOTNET_VERSION" in
                        8.*)
                            echo ".NET 8 detected."
                            ;;
                        *)
                            echo "ERROR: .NET 8 SDK required."
                            exit 1
                            ;;
                    esac

                    echo ""
                    echo "Checking systemd service..."

                    if ! systemctl cat "${SERVICE_NAME}.service" >/dev/null 2>&1; then

                        echo "ERROR: Service not found:"
                        echo "${SERVICE_NAME}.service"
                    
                        echo ""
                        echo "Available SSManagement services:"
                    
                        systemctl list-unit-files | grep -i ssmanagement || true
                    
                        exit 1
                    fi
                    
                    echo "Service found: ${SERVICE_NAME}.service"
                    echo "Service found: $SERVICE_NAME"

                    echo ""
                    echo "Checking deployment directory..."

                    mkdir -p "$DEPLOY_PATH"

                    ls -ld "$DEPLOY_PATH"

                    echo ""
                    echo "Checking disk space..."

                    df -h "$DEPLOY_PATH"

                    echo ""
                    echo "Checking current API..."

                    if systemctl is-active --quiet "$SERVICE_NAME"; then
                        echo "Current API is running."
                    else
                        echo "WARNING: Current API service is not running."
                    fi

                    echo ""
                    echo "Checking port $API_PORT..."

                    ss -lnt | grep ":${API_PORT}" || true

                    echo ""
                    echo "Pre-check completed successfully."
                '''
            }
        }


        stage('Checkout') {

            steps {

                echo "Checking out GitHub source..."

                checkout scm
            }
        }


        stage('Validate Source') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "=========================================="
                    echo " Validating Source"
                    echo "=========================================="

                    echo ""
                    echo "Solution files:"

                    find . -type f -name "*.sln" -print

                    echo ""
                    echo "Project files:"

                    find . -type f -name "*.csproj" -print

                    PROJECT_COUNT=$(find . -type f -name "*.csproj" | wc -l)

                    if [ "$PROJECT_COUNT" -eq 0 ]; then
                        echo "ERROR: No .csproj file found."
                        exit 1
                    fi

                    echo ""
                    echo "Found $PROJECT_COUNT project(s)."
                '''
            }
        }


        stage('Clean') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "Cleaning publish directory..."

                    rm -rf "$PUBLISH_PATH"

                    mkdir -p "$PUBLISH_PATH"

                    echo "Running dotnet clean..."

                    dotnet clean -c Release
                '''
            }
        }


        stage('Restore') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "Running dotnet restore..."

                    dotnet restore
                '''
            }
        }


        stage('Build') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "Building application..."

                    dotnet build \
                        -c Release \
                        --no-restore \
                        --nologo
                '''
            }
        }


        stage('Publish') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "Publishing application..."

                    rm -rf "$PUBLISH_PATH"

                    mkdir -p "$PUBLISH_PATH"

                    dotnet publish \
                        -c Release \
                        --no-restore \
                        --no-build \
                        -o "$PUBLISH_PATH" \
                        --nologo

                    echo ""
                    echo "Published files:"
                    ls -lah "$PUBLISH_PATH"

                    echo ""
                    echo "Checking application DLL..."

                    if [ ! -f "$PUBLISH_PATH/SSProjectSolution.dll" ]; then

                        echo "ERROR:"
                        echo "SSProjectSolution.dll was not found."

                        echo ""
                        echo "Available DLL files:"

                        find "$PUBLISH_PATH" \
                            -maxdepth 1 \
                            -type f \
                            -name "*.dll" \
                            -print

                        exit 1
                    fi

                    echo ""
                    echo "SSProjectSolution.dll found."

                    echo ""
                    echo "Checking runtimeconfig..."

                    RUNTIME_FILE=$(find "$PUBLISH_PATH" \
                        -maxdepth 1 \
                        -name "SSProjectSolution.runtimeconfig.json" \
                        -print)

                    if [ -z "$RUNTIME_FILE" ]; then
                        echo "ERROR: runtimeconfig file not found."
                        exit 1
                    fi

                    echo "Runtime config found."

                    echo ""
                    echo "Publish validation successful."
                '''
            }
        }


        stage('Backup') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    BACKUP_DIR="/var/www/SSManagement/DEV/API_Backup_$(date +%Y%m%d_%H%M%S)"

                    echo "Creating backup:"
                    echo "$BACKUP_DIR"

                    mkdir -p "$BACKUP_DIR"

                    if [ -d "$DEPLOY_PATH" ]; then

                        cp -a "$DEPLOY_PATH"/. "$BACKUP_DIR"/ 2>/dev/null || true

                        echo "Backup completed."

                    else

                        echo "No previous deployment found."

                    fi
                '''
            }
        }


        stage('Stop API') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "Stopping service:"
                    echo "$SERVICE_NAME"

                    sudo -n /usr/bin/systemctl stop "$SERVICE_NAME"

                    sleep 2

                    if systemctl is-active --quiet "$SERVICE_NAME"; then

                        echo "ERROR: Service did not stop."

                        exit 1
                    fi

                    echo "Service stopped successfully."
                '''
            }
        }


        stage('Deploy') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "Deploying application..."

                    mkdir -p "$DEPLOY_PATH"

                    echo "Removing old files..."

                    rm -rf "$DEPLOY_PATH"/*

                    echo "Copying published files..."

                    cp -a "$PUBLISH_PATH"/. "$DEPLOY_PATH"/

                    echo "Setting ownership..."

                    chown -R root:root "$DEPLOY_PATH"

                    echo ""
                    echo "Checking deployed DLL..."

                    if [ ! -f "$DEPLOY_PATH/SSProjectSolution.dll" ]; then

                        echo "ERROR: SSProjectSolution.dll missing after deployment."

                        exit 1
                    fi

                    echo ""
                    echo "Deployment contents:"

                    ls -lah "$DEPLOY_PATH"

                    echo ""
                    echo "Deployment completed successfully."
                '''
            }
        }


        stage('Start API') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "Reloading systemd..."

                    sudo -n /usr/bin/systemctl daemon-reload

                    echo "Starting service..."

                    sudo -n /usr/bin/systemctl start "$SERVICE_NAME"

                    echo "Waiting for application..."

                    sleep 5

                    sudo -n /usr/bin/systemctl status \
                        "$SERVICE_NAME" \
                        --no-pager
                '''
            }
        }


        stage('Verify Service') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "Checking service..."

                    if ! sudo -n /usr/bin/systemctl is-active --quiet "$SERVICE_NAME"; then

                        echo "ERROR: API service is not running."

                        echo ""
                        echo "===== SERVICE LOGS ====="

                        sudo -n /usr/bin/journalctl \
                            -u "$SERVICE_NAME" \
                            --no-pager \
                            -n 100

                        exit 1
                    fi

                    echo "Service is active."
                '''
            }
        }


        stage('Verify Port') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo "Checking port $API_PORT..."

                    API_STARTED=false

                    for i in {1..15}; do

                        if ss -lnt | grep -q ":${API_PORT} "; then

                            echo "Port $API_PORT is listening."

                            API_STARTED=true

                            break
                        fi

                        echo "Waiting for API..."
                        sleep 2

                    done

                    if [ "$API_STARTED" != "true" ]; then

                        echo "ERROR: API is not listening on port $API_PORT."

                        echo ""
                        echo "===== SERVICE STATUS ====="

                        sudo -n /usr/bin/systemctl status \
                            "$SERVICE_NAME" \
                            --no-pager || true

                        echo ""
                        echo "===== SERVICE LOGS ====="

                        sudo -n /usr/bin/journalctl \
                            -u "$SERVICE_NAME" \
                            --no-pager \
                            -n 100 || true

                        exit 1
                    fi
                '''
            }
        }


        stage('Final Validation') {

            steps {

                sh '''
                    #!/bin/bash
                    set -Eeuo pipefail

                    echo ""
                    echo "=========================================="
                    echo " DEPLOYMENT SUCCESSFUL"
                    echo "=========================================="

                    echo ""
                    echo "Application : $APP_NAME"
                    echo "Environment : $DOTNET_ENVIRONMENT"
                    echo "Service     : $SERVICE_NAME"
                    echo "Port        : $API_PORT"
                    echo "Path        : $DEPLOY_PATH"

                    echo ""
                    echo "Service status:"

                    sudo -n /usr/bin/systemctl is-active "$SERVICE_NAME"

                    echo ""
                    echo "Port status:"

                    ss -lnt | grep ":${API_PORT} "

                    echo ""
                    echo "Application DLL:"

                    ls -lh "$DEPLOY_PATH/SSProjectSolution.dll"

                    echo ""
                    echo "Deployment completed successfully."
                '''
            }
        }
    }


    post {

        success {

            echo """
==========================================
SSManagement DEV API DEPLOYMENT SUCCESS
==========================================

Application : ${APP_NAME}
Environment : ${DOTNET_ENVIRONMENT}
Service     : ${SERVICE_NAME}
Port        : ${API_PORT}
Path        : ${DEPLOY_PATH}
Build       : ${BUILD_NUMBER}

Deployment successful.
"""
        }


        failure {

            echo """
==========================================
SSManagement DEV API DEPLOYMENT FAILED
==========================================

Build: ${BUILD_NUMBER}

Collecting diagnostics...
"""

            sh '''
                #!/bin/bash

                set +e

                echo ""
                echo "===== SERVICE STATUS ====="

                sudo -n /usr/bin/systemctl status \
                    "$SERVICE_NAME" \
                    --no-pager || true

                echo ""
                echo "===== SERVICE LOGS ====="

                sudo -n /usr/bin/journalctl \
                    -u "$SERVICE_NAME" \
                    --no-pager \
                    -n 100 || true

                echo ""
                echo "===== PORT ====="

                ss -lnt | grep ":${API_PORT}" || true

                echo ""
                echo "===== DISK SPACE ====="

                df -h

                echo ""
                echo "===== DEPLOYMENT DIRECTORY ====="

                ls -lah "$DEPLOY_PATH" || true
            '''
        }


        always {

            echo "Cleaning Jenkins workspace..."

            cleanWs(
                deleteDirs: true,
                disableDeferredWipeout: true,
                notFailBuild: true
            )
        }
    }
}
