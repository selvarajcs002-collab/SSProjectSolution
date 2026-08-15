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

        BACKUP_ROOT = "/var/www/SSManagement/DEV"
    }

    stages {

        // ============================================================
        // 1. PRE-CHECK
        // ============================================================

        stage('Pre-Check') {

            steps {

                sh '''#!/bin/bash

                    echo "=========================================="
                    echo " SSManagement API DEV Deployment"
                    echo "=========================================="

                    echo ""
                    echo "Checking .NET SDK..."

                    if ! command -v dotnet >/dev/null 2>&1; then
                        echo "ERROR: dotnet is not installed."
                        exit 1
                    fi

                    DOTNET_VERSION=$(dotnet --version)

                    echo "Installed .NET SDK: $DOTNET_VERSION"

                    case "$DOTNET_VERSION" in
                        8.*)
                            echo ".NET 8 SDK detected."
                            ;;
                        *)
                            echo "ERROR: .NET 8 SDK is required."
                            exit 1
                            ;;
                    esac


                    echo ""
                    echo "Checking systemd service..."

                    if ! systemctl cat "${SERVICE_NAME}.service" >/dev/null 2>&1; then

                        echo "ERROR: Service does not exist:"
                        echo "${SERVICE_NAME}.service"

                        echo ""
                        echo "Available SSManagement services:"

                        systemctl list-unit-files | grep -i ssmanagement || true

                        exit 1
                    fi

                    echo "Service found: ${SERVICE_NAME}.service"


                    echo ""
                    echo "Checking deployment directory..."

                    if ! mkdir -p "$DEPLOY_PATH"; then
                        echo "ERROR: Cannot create/access deployment directory."
                        exit 1
                    fi

                    if [ ! -w "$DEPLOY_PATH" ]; then
                        echo "ERROR: Deployment directory is not writable:"
                        echo "$DEPLOY_PATH"
                        exit 1
                    fi

                    echo "Deployment directory is available."


                    echo ""
                    echo "Checking disk space..."

                    df -h "$DEPLOY_PATH"

                    AVAILABLE_KB=$(df -Pk "$DEPLOY_PATH" | awk 'NR==2 {print $4}')

                    if [ "$AVAILABLE_KB" -lt 1048576 ]; then
                        echo "ERROR: Less than 1 GB disk space available."
                        exit 1
                    fi

                    echo "Disk space is sufficient."


                    echo ""
                    echo "Checking current service..."

                    if systemctl is-active --quiet "$SERVICE_NAME"; then
                        echo "Current API is running."
                    else
                        echo "WARNING: Current API is not running."
                    fi


                    echo ""
                    echo "Checking port $API_PORT..."

                    if ss -lnt | grep -q ":${API_PORT} "; then
                        echo "Port $API_PORT is currently in use."
                    else
                        echo "Port $API_PORT is currently free."
                    fi


                    echo ""
                    echo "Pre-check completed successfully."
                '''
            }
        }


        // ============================================================
        // 2. CHECKOUT
        // ============================================================

        stage('Checkout') {

            steps {

                echo "Checking out GitHub source..."

                checkout scm
            }
        }


        // ============================================================
        // 3. VALIDATE SOURCE
        // ============================================================

        stage('Validate Source') {

            steps {

                sh '''#!/bin/bash

                    echo "=========================================="
                    echo " Validating Source"
                    echo "=========================================="

                    PROJECT_COUNT=$(find . -type f -name "*.csproj" | wc -l)

                    if [ "$PROJECT_COUNT" -eq 0 ]; then
                        echo "ERROR: No .csproj file found."
                        exit 1
                    fi

                    echo "Found $PROJECT_COUNT project(s)."

                    echo ""
                    echo "Project files:"

                    find . -type f -name "*.csproj" -print

                    if [ ! -f "SSProjectSolution.csproj" ]; then
                        echo "WARNING: SSProjectSolution.csproj not found at repository root."
                        echo "Using discovered project files."
                    fi
                '''
            }
        }


        // ============================================================
        // 4. CLEAN
        // ============================================================

        stage('Clean') {

            steps {

                sh '''#!/bin/bash

                    echo "Cleaning previous publish output..."

                    rm -rf "$PUBLISH_PATH"

                    mkdir -p "$PUBLISH_PATH"

                    echo "Running dotnet clean..."

                    dotnet clean -c Release --nologo

                    echo "Clean completed."
                '''
            }
        }


        // ============================================================
        // 5. RESTORE
        // ============================================================

        stage('Restore') {

            steps {

                sh '''#!/bin/bash

                    echo "Restoring NuGet packages..."

                    dotnet restore

                    echo "Restore completed."
                '''
            }
        }


        // ============================================================
        // 6. BUILD
        // ============================================================

        stage('Build') {

            steps {

                sh '''#!/bin/bash

                    echo "Building application..."

                    dotnet build \
                        -c Release \
                        --no-restore \
                        --nologo

                    echo "Build completed successfully."
                '''
            }
        }


        // ============================================================
        // 7. PUBLISH
        // ============================================================

        stage('Publish') {

            steps {

                sh '''#!/bin/bash

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
                    echo "Checking published application..."

                    if [ ! -f "$PUBLISH_PATH/SSProjectSolution.dll" ]; then

                        echo "ERROR: SSProjectSolution.dll was not generated."

                        echo ""
                        echo "Published DLL files:"

                        find "$PUBLISH_PATH" \
                            -maxdepth 1 \
                            -type f \
                            -name "*.dll" \
                            -print

                        exit 1
                    fi


                    if [ ! -f "$PUBLISH_PATH/SSProjectSolution.runtimeconfig.json" ]; then
                        echo "ERROR: runtimeconfig.json was not generated."
                        exit 1
                    fi


                    echo "Published application is valid."

                    echo ""
                    echo "Publish size:"

                    du -sh "$PUBLISH_PATH"
                '''
            }
        }


        // ============================================================
        // 8. BACKUP
        // ============================================================

        stage('Backup') {

            steps {

                sh '''#!/bin/bash

                    BACKUP_DIR="${BACKUP_ROOT}/API_Backup_$(date +%Y%m%d_%H%M%S)"

                    echo "Creating backup:"
                    echo "$BACKUP_DIR"

                    mkdir -p "$BACKUP_DIR"

                    if [ -d "$DEPLOY_PATH" ] && [ "$(ls -A "$DEPLOY_PATH" 2>/dev/null)" ]; then

                        cp -a "$DEPLOY_PATH"/. "$BACKUP_DIR"/

                        echo "Backup completed."

                    else

                        echo "No existing deployment found."
                        echo "Nothing to backup."

                    fi
                '''
            }
        }


        // ============================================================
        // 9. STOP API
        // ============================================================

        stage('Stop API') {

            steps {

                sh '''#!/bin/bash

                    echo "Stopping API service..."

                    sudo -n /usr/bin/systemctl stop "$SERVICE_NAME"

                    sleep 2

                    if systemctl is-active --quiet "$SERVICE_NAME"; then

                        echo "ERROR: API service did not stop."

                        systemctl status "$SERVICE_NAME" --no-pager || true

                        exit 1
                    fi

                    echo "API service stopped."
                '''
            }
        }


        // ============================================================
        // 10. DEPLOY
        // ============================================================

        stage('Deploy') {

            steps {

                sh '''#!/bin/bash

                    echo "Deploying application..."

                    if [ ! -f "$PUBLISH_PATH/SSProjectSolution.dll" ]; then
                        echo "ERROR: Published DLL does not exist."
                        exit 1
                    fi


                    echo "Removing old deployment..."

                    rm -rf "$DEPLOY_PATH"/*


                    echo "Copying new files..."

                    cp -a "$PUBLISH_PATH"/. "$DEPLOY_PATH"/


                    echo "Setting ownership..."

                    chown -R root:root "$DEPLOY_PATH"


                    echo "Checking deployed DLL..."

                    if [ ! -f "$DEPLOY_PATH/SSProjectSolution.dll" ]; then

                        echo "ERROR: Deployment failed."
                        echo "SSProjectSolution.dll is missing."

                        exit 1
                    fi


                    echo "Deployment completed."
                '''
            }
        }


        // ============================================================
        // 11. START API
        // ============================================================

        stage('Start API') {

            steps {

                sh '''#!/bin/bash

                    echo "Reloading systemd..."

                    sudo -n /usr/bin/systemctl daemon-reload


                    echo "Starting API service..."

                    sudo -n /usr/bin/systemctl start "$SERVICE_NAME"


                    echo "Waiting for API to start..."

                    sleep 5


                    if ! sudo -n /usr/bin/systemctl is-active --quiet "$SERVICE_NAME"; then

                        echo "ERROR: API service failed to start."

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
                            -n 50 || true

                        exit 1
                    fi


                    echo "API service started successfully."
                '''
            }
        }


        // ============================================================
        // 12. VERIFY SERVICE
        // ============================================================

        stage('Verify Service') {

            steps {

                sh '''#!/bin/bash

                    echo "Verifying service..."

                    if ! sudo -n /usr/bin/systemctl is-active --quiet "$SERVICE_NAME"; then

                        echo "ERROR: Service is not active."

                        sudo -n /usr/bin/systemctl status \
                            "$SERVICE_NAME" \
                            --no-pager || true

                        exit 1
                    fi

                    echo "Service is active."
                '''
            }
        }


        // ============================================================
        // 13. VERIFY PORT
        // ============================================================

        stage('Verify Port') {

            steps {

                sh '''#!/bin/bash

                    echo "Checking port $API_PORT..."

                    API_STARTED=false


                    for i in {1..15}; do

                        if ss -lnt | grep -q ":${API_PORT} "; then

                            echo "Port $API_PORT is listening."

                            API_STARTED=true

                            break
                        fi

                        echo "Waiting for API... attempt $i/15"

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
                            -n 50 || true

                        echo ""
                        echo "===== PORTS ====="

                        ss -lnt || true

                        exit 1
                    fi
                '''
            }
        }


        // ============================================================
        // 14. FINAL VALIDATION
        // ============================================================

        stage('Final Validation') {

            steps {

                sh '''#!/bin/bash

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


    // ================================================================
    // POST ACTIONS
    // ================================================================

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

Application : ${APP_NAME}
Service     : ${SERVICE_NAME}
Build       : ${BUILD_NUMBER}

Collecting diagnostics...
"""


            sh '''#!/bin/bash

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

                df -h || true


                echo ""
                echo "===== DEPLOYMENT DIRECTORY ====="

                ls -lah "$DEPLOY_PATH" || true


                echo ""
                echo "===== PUBLISH DIRECTORY ====="

                ls -lah "$PUBLISH_PATH" || true

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
