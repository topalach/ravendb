pipeline {
  agent any

  stages {

    stage('Build') {
        steps {
            dir('src/Raven.Server') {
                sh 'dotnet build'
            }
      }
    }

  }
}