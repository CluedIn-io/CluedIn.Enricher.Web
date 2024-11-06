      if (!(Test-Path GitVersion.yml)) { "Missing GitVersion.yml"; return }
      if (!(Test-Path Packages.props)) { "Missing Packages.props"; return }
      
      $x = [xml](get-content Packages.props)
      $cluedInVersion = $x.SelectSingleNode('//_CluedIn/text()')
      
      if($cluedInVersion -eq $null) { "Can't find _CluedIn in Packages.props"; return }
      
      if (!($cluedInVersion.value -match '^([0-9]+\.[0-9]+)')) { "_CluedIn did not match version format"; return }
      
      $nextVersion = [Double]$Matches[1]
      
      Install-Module powershell-yaml -RequiredVersion 0.4.7 -AcceptLicense -Scope CurrentUser -Force
      Import-Module powershell-yaml -RequiredVersion 0.4.7
      
      $config = Get-Content GitVersion.yml | ConvertFrom-Yaml -Ordered
      if (($config.'next-version' -eq $null) -or ([Double]$config.'next-version' -lt $nextVersion)) {
        $config.'next-version' = $nextVersion
      }
      
      if ($config.branches.develop -eq $null) {
        $config.branches.develop = @{}
      }
      $config.branches.develop.increment='Patch'
      
      if ($config.branches.'pull-request' -eq $null) {
        $config.branches.'pull-request' = @{}
      }
      $config.branches.'pull-request'.tag='alpha-pr'
      $config.branches.'pull-request'.increment='Patch'
      
      if ($config.branches.unknown -eq $null) {
        $config.branches.unknown = @{}
      }
      $config.branches.unknown.increment = 'Patch'
      $config.branches.unknown.regex = '(?<BranchName>.+)'
      $config.branches.unknown.tag = 'alpha-{BranchName}'
      $config.branches.unknown.'source-branches' = @('main','develop')
      
      $config | ConvertTo-Yaml | Tee-Object -FilePath GitVersion.yml