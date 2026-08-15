pipeline {

    agent any

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

        API_URL = "http://127.0.0.1:5000"

        BACKUP_ROOT = "/var/www/SSManagement/DEV"

        MIN_FREE_SPACE_MB = "1000"
    }

    stages {

        /*
         * ============================================================
         * PRE-CHECK
         * ============================================================
         */

        stage('Pre-Check') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "=========================================="
                    echo "        SSManagement API DEV DEPLOY"
                    echo "=========================================="

                    echo ""
                    echo "Checking Linux..."
                    uname -a

                    echo ""
                    echo "Checking .NET SDK..."

                    if ! command -v dotnet >/dev/null 2>&1; then
                        echo "ERROR: dotnet command not found."
                        exit 1
                    fi

                    dotnet --version
                    dotnet --info

                    echo ""
                    echo "Checking required .NET 8 SDK..."

                    DOTNET_MAJOR=$(dotnet --version | cut -d. -f1)

                    if [ "$DOTNET_MAJOR" != "8" ]; then
                        echo "ERROR: .NET 8 SDK is required."
                        echo "Found: $(dotnet --version)"
                        exit 1
                    fi

                    echo ""
                    echo "Checking disk space..."

                    AVAILABLE_MB=$(df -Pm "$WORKSPACE" | awk 'NR==2 {print $4}')

                    echo "Available disk space: ${AVAILABLE_MB} MB"

                    if [ "$AVAILABLE_MB" -lt "$MIN_FREE_SPACE_MB" ]; then
                        echo "ERROR: Insufficient disk space."
                        echo "Required minimum: ${MIN_FREE_SPACE_MB} MB"
                        exit 1
                    fi

                    echo ""
                    echo "Checking systemd..."

                    if ! command -v systemctl >/dev/null 2>&1; then
                        echo "ERROR: systemctl not available."
                        exit 1
                    fi

                    echo ""
                    echo "Checking service: $SERVICE_NAME"

                    if ! systemctl list-unit-files | grep -q "^${SERVICE_NAME}.service"; then
                        echo "ERROR: systemd service not found:"
                        echo "$SERVICE_NAME"

                        echo ""
                        echo "Available SSManagement services:"
                        systemctl list-unit-files | grep -i ssmanagement || true

                        exit 1
                    fi

                    echo ""
                    echo "Service found."

                    systemctl cat "$SERVICE_NAME"

                    echo ""
                    echo "Pre-check completed successfully."
                '''
            }
        }


        /*
         * ============================================================
         * CHECKOUT
         * ============================================================
         */

        stage('Checkout') {

            steps {

                echo 'Checking out source code...'

                checkout scm
            }
        }


        /*
         * ============================================================
         * SOURCE VALIDATION
         * ============================================================
         */

        stage('Validate Source') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "Searching for .NET solution/project..."

                    echo ""
                    echo "Solutions:"
                    find . -maxdepth 3 -type f -name "*.sln" -print || true

                    echo ""
                    echo "Projects:"
                    find . -maxdepth 5 -type f -name "*.csproj" -print || true

                    PROJECT_COUNT=$(find . -type f -name "*.csproj" | wc -l)

                    if [ "$PROJECT_COUNT" -eq 0 ]; then
                        echo "ERROR: No .csproj file found."
                        exit 1
                    fi

                    echo ""
                    echo "Found $PROJECT_COUNT project(s)."

                    if [ ! -f "*.sln" ] && ! find . -maxdepth 2 -type f -name "*.sln" | grep -q .; then
                        echo "WARNING: No solution file found at repository root."
                        echo "dotnet restore/build/publish may require project selection."
                    fi
                '''
            }
        }


        /*
         * ============================================================
         * CLEAN
         * ============================================================
         */

        stage('Clean') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "Cleaning Jenkins publish directory..."

                    rm -rf "$PUBLISH_PATH"

                    mkdir -p "$PUBLISH_PATH"

                    echo "Cleaning .NET build..."

                    dotnet clean -c Release
                '''
            }
        }


        /*
         * ============================================================
         * RESTORE
         * ============================================================
         */

        stage('Restore') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "Running dotnet restore..."

                    dotnet restore

                    echo "Restore completed successfully."
                '''
            }
        }


        /*
         * ============================================================
         * BUILD
         * ============================================================
         */

        stage('Build') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "Building application..."

                    dotnet build \
                        -c Release \
                        --no-restore \
                        --nologo

                    echo "Build completed successfully."
                '''
            }
        }


        /*
         * ============================================================
         * PUBLISH
         * ============================================================
         */

        stage('Publish') {

            steps {

                sh '''
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

                    find "$PUBLISH_PATH" -maxdepth 2 -type f | sort

                    echo ""
                    echo "Checking publish directory..."

                    if [ ! -d "$PUBLISH_PATH" ]; then
                        echo "ERROR: Publish directory does not exist."
                        exit 1
                    fi

                    FILE_COUNT=$(find "$PUBLISH_PATH" -type f | wc -l)

                    if [ "$FILE_COUNT" -eq 0 ]; then
                        echo "ERROR: Publish directory is empty."
                        exit 1
                    fi

                    echo "Published file count: $FILE_COUNT"

                    echo ""
                    echo "Checking for runtimeconfig..."

                    RUNTIME_CONFIG=$(find "$PUBLISH_PATH" -maxdepth 1 -name "*.runtimeconfig.json" | head -1)

                    if [ -z "$RUNTIME_CONFIG" ]; then
                        echo "ERROR: No *.runtimeconfig.json found."
                        exit 1
                    fi

                    echo "Runtime config: $RUNTIME_CONFIG"

                    echo ""
                    echo "Checking for application DLL..."

                    APP_DLL=$(find "$PUBLISH_PATH" -maxdepth 1 -type f -name "*.dll" | head -1)

                    if [ -z "$APP_DLL" ]; then
                        echo "ERROR: No application DLL found."
                        exit 1
                    fi

                    echo "Application DLL: $APP_DLL"

                    echo "$APP_DLL" > "$WORKSPACE/application-dll.txt"

                    echo ""
                    echo "Publish validation successful."
                '''
            }
        }


        /*
         * ============================================================
         * BACKUP
         * ============================================================
         */

        stage('Backup Current Deployment') {

            steps {

                script {

                    env.BACKUP_DIR =
                        "${BACKUP_ROOT}/API_Backup_${new Date().format('yyyyMMdd_HHmmss')}"

                    echo "Backup directory:"
                    echo env.BACKUP_DIR
                }

                sh '''
                    set -Eeuo pipefail

                    echo "Creating backup..."

                    mkdir -p "$BACKUP_DIR"

                    if [ -d "$DEPLOY_PATH" ]; then

                        echo "Backing up existing deployment..."

                        cp -a "$DEPLOY_PATH"/. "$BACKUP_DIR"/

                        echo "Backup completed."

                    else

                        echo "No existing deployment found."
                        echo "Fresh deployment."
                    fi

                    echo ""
                    echo "Backup contents:"

                    find "$BACKUP_DIR" -maxdepth 2 -type f | head -50 || true
                '''
            }
        }


        /*
         * ============================================================
         * STOP SERVICE
         * ============================================================
         */

        stage('Stop API') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "Stopping service: $SERVICE_NAME"

                    sudo systemctl stop "$SERVICE_NAME"

                    echo "Checking service status..."

                    if systemctl is-active --quiet "$SERVICE_NAME"; then
                        echo "ERROR: Service is still running."
                        exit 1
                    fi

                    echo "Service stopped successfully."
                '''
            }
        }


        /*
         * ============================================================
         * STAGE DEPLOYMENT
         * ============================================================
         */

        stage('Prepare Deployment') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    STAGING_PATH="${DEPLOY_PATH}_staging_${BUILD_NUMBER}"

                    echo "Creating staging directory:"
                    echo "$STAGING_PATH"

                    rm -rf "$STAGING_PATH"

                    mkdir -p "$STAGING_PATH"

                    cp -a "$PUBLISH_PATH"/. "$STAGING_PATH"/

                    echo ""
                    echo "Validating staging deployment..."

                    if [ ! -f "$STAGING_PATH"/*.dll ]; then
                        echo "ERROR: No DLL found in staging directory."
                        exit 1
                    fi

                    if [ ! -f "$STAGING_PATH"/*.runtimeconfig.json ]; then
                        echo "ERROR: runtimeconfig.json missing."
                        exit 1
                    fi

                    echo ""
                    echo "Staging deployment ready."
                '''
            }
        }


        /*
         * ============================================================
         * DEPLOY
         * ============================================================
         */

        stage('Deploy') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    STAGING_PATH="${DEPLOY_PATH}_staging_${BUILD_NUMBER}"

                    echo "Deploying application..."

                    mkdir -p "$DEPLOY_PATH"

                    rm -rf "$DEPLOY_PATH"/*

                    cp -a "$STAGING_PATH"/. "$DEPLOY_PATH"/

                    rm -rf "$STAGING_PATH"

                    echo ""
                    echo "Setting ownership..."

                    chown -R root:root "$DEPLOY_PATH"

                    echo ""
                    echo "Setting permissions..."

                    find "$DEPLOY_PATH" -type d -exec chmod 755 {} \\;
                    find "$DEPLOY_PATH" -type f -exec chmod 644 {} \\;

                    echo ""
                    echo "Deployment copied successfully."

                    echo ""
                    echo "Deployment files:"

                    ls -lah "$DEPLOY_PATH"
                '''
            }
        }


        /*
         * ============================================================
         * START SERVICE
         * ============================================================
         */

        stage('Start API') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "Reloading systemd..."

                    sudo systemctl daemon-reload

                    echo "Starting service: $SERVICE_NAME"

                    sudo systemctl start "$SERVICE_NAME"

                    echo "Waiting for service..."

                    sleep 5

                    echo ""
                    echo "Service status:"

                    sudo systemctl status "$SERVICE_NAME" --no-pager
                '''
            }
        }


        /*
         * ============================================================
         * VERIFY SERVICE
         * ============================================================
         */

        stage('Verify Service') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "Checking service..."

                    if ! sudo systemctl is-active --quiet "$SERVICE_NAME"; then

                        echo "ERROR: Service is not active."

                        echo ""
                        echo "Recent service logs:"

                        sudo journalctl \
                            -u "$SERVICE_NAME" \
                            --no-pager \
                            -n 100

                        exit 1
                    fi

                    echo "Service is active."
                '''
            }
        }


        /*
         * ============================================================
         * VERIFY PORT
         * ============================================================
         */

        stage('Verify Port') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "Checking port $API_PORT..."

                    for i in {1..12}; do

                        if ss -ltn | grep -q ":${API_PORT} "; then
                            echo "Port $API_PORT is listening."
                            break
                        fi

                        echo "Waiting for port $API_PORT..."
                        sleep 2

                        if [ "$i" -eq 12 ]; then
                            echo "ERROR: API did not start on port $API_PORT."

                            sudo journalctl \
                                -u "$SERVICE_NAME" \
                                --no-pager \
                                -n 100

                            exit 1
                        fi

                    done
                '''
            }
        }


        /*
         * ============================================================
         * HTTP HEALTH CHECK
         * ============================================================
         */

        stage('Health Check') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo "Testing API..."

                    if command -v curl >/dev/null 2>&1; then

                        HTTP_CODE=$(curl \
                            -s \
                            -o /dev/null \
                            -w "%{http_code}" \
                            --connect-timeout 5 \
                            --max-time 10 \
                            "$API_URL" || true)

                        echo "HTTP response: $HTTP_CODE"

                        if [ "$HTTP_CODE" = "000" ]; then
                            echo "ERROR: API is not responding."
                            exit 1
                        fi

                        echo "HTTP endpoint is responding."

                    else

                        echo "WARNING: curl is not installed."
                        echo "Skipping HTTP health check."

                    fi
                '''
            }
        }


        /*
         * ============================================================
         * FINAL VALIDATION
         * ============================================================
         */

        stage('Final Validation') {

            steps {

                sh '''
                    set -Eeuo pipefail

                    echo ""
                    echo "=========================================="
                    echo "        FINAL DEPLOYMENT VALIDATION"
                    echo "=========================================="

                    echo ""
                    echo "Application:"
                    echo "$APP_NAME"

                    echo ""
                    echo "Environment:"
                    echo "$DOTNET_ENVIRONMENT"

                    echo ""
                    echo "Deployment path:"
                    echo "$DEPLOY_PATH"

                    echo ""
                    echo "Service:"
                    echo "$SERVICE_NAME"

                    echo ""
                    echo "Port:"
                    echo "$API_PORT"

                    echo ""
                    echo "Service status:"

                    sudo systemctl is-active "$SERVICE_NAME"

                    echo ""
                    echo "Port status:"

                    ss -lntp | grep ":${API_PORT}" || true

                    echo ""
                    echo "Deployment completed successfully."
                '''
            }
        }
    }


    /*
     * ================================================================
     * POST ACTIONS
     * ================================================================
     */

    post {

        success {

            echo """
            ==========================================
            SSManagement DEV API DEPLOYMENT SUCCESS
            ==========================================

            Application : ${APP_NAME}
            Environment : ${DOTNET_ENVIRONMENT}
            Port        : ${API_PORT}
            Service     : ${SERVICE_NAME}
            Deploy Path : ${DEPLOY_PATH}
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

            Attempting to collect diagnostic information...
            """

            sh '''
                set +e

                echo ""
                echo "===== SERVICE STATUS ====="

                sudo systemctl status "$SERVICE_NAME" --no-pager

                echo ""
                echo "===== SERVICE LOGS ====="

                sudo journalctl \
                    -u "$SERVICE_NAME" \
                    --no-pager \
                    -n 150

                echo ""
                echo "===== PORT 5000 ====="

                ss -lntp | grep ":${API_PORT}" || true

                echo ""
                echo "===== DISK SPACE ====="

                df -h

                echo ""
                echo "===== DEPLOYMENT DIRECTORY ====="

                ls -lah "$DEPLOY_PATH" || true

                echo ""
                echo "===== ROLLBACK ====="

                if [ -n "${BACKUP_DIR:-}" ] && [ -d "${BACKUP_DIR:-}" ]; then

                    echo "Backup found:"
                    echo "$BACKUP_DIR"

                    echo "Stopping failed application..."

                    sudo systemctl stop "$SERVICE_NAME" || true

                    echo "Removing failed deployment..."

                    rm -rf "$DEPLOY_PATH"

                    echo "Restoring backup..."

                    mkdir -p "$DEPLOY_PATH"

                    cp -a "$BACKUP_DIR"/. "$DEPLOY_PATH"/

                    chown -R root:root "$DEPLOY_PATH"

                    find "$DEPLOY_PATH" -type d -exec chmod 755 {} \\;
                    find "$DEPLOY_PATH" -type f -exec chmod 644 {} \\;

                    echo "Starting previous version..."

                    sudo systemctl daemon-reload
                    sudo systemctl start "$SERVICE_NAME"

                    sleep 5

                    if sudo systemctl is-active --quiet "$SERVICE_NAME"; then
                        echo "ROLLBACK SUCCESSFUL."
                    else
                        echo "ROLLBACK FAILED."

                        sudo systemctl status \
                            "$SERVICE_NAME" \
                            --no-pager || true
                    fi

                else

                    echo "No backup available."
                    echo "Rollback not possible."

                fi
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
