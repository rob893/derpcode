param location string = resourceGroup().location
param vmName string = 'linuxvm'
param adminUsername string = 'azureuser'

@description('SSH public key for the admin user. Password authentication is disabled.')
param adminSshPublicKey string

@description('Source address prefix (CIDR or single IP) allowed to SSH to the VM. Set to your operator IP/CIDR; do not use \'*\' in production.')
param sshSourceAddressPrefix string

@description('Source address prefix allowed to reach HTTP/HTTPS. Defaults to the public internet.')
param httpSourceAddressPrefix string = 'Internet'

var vmSize = 'Standard_B1s'
var ubuntuImage = {
  publisher: 'Canonical'
  offer: '0001-com-ubuntu-server-focal'
  sku: '20_04-lts-gen2'
  version: 'latest'
}
var publicIpName = '${vmName}-pip'
var nicName = '${vmName}-nic'
var vnetName = '${vmName}-vnet'
var subnetName = 'default'
var nsgName = '${vmName}-nsg'

resource nsg 'Microsoft.Network/networkSecurityGroups@2023-02-01' = {
  name: nsgName
  location: location
  properties: {
    securityRules: [
      {
        name: 'AllowSshInbound'
        properties: {
          priority: 1000
          access: 'Allow'
          direction: 'Inbound'
          protocol: 'Tcp'
          sourcePortRange: '*'
          sourceAddressPrefix: sshSourceAddressPrefix
          destinationPortRange: '22'
          destinationAddressPrefix: '*'
          description: 'SSH access restricted to the operator source prefix.'
        }
      }
      {
        name: 'AllowHttpInbound'
        properties: {
          priority: 1010
          access: 'Allow'
          direction: 'Inbound'
          protocol: 'Tcp'
          sourcePortRange: '*'
          sourceAddressPrefix: httpSourceAddressPrefix
          destinationPortRange: '80'
          destinationAddressPrefix: '*'
          description: 'HTTP for ACME challenges and HTTPS redirect.'
        }
      }
      {
        name: 'AllowHttpsInbound'
        properties: {
          priority: 1020
          access: 'Allow'
          direction: 'Inbound'
          protocol: 'Tcp'
          sourcePortRange: '*'
          sourceAddressPrefix: httpSourceAddressPrefix
          destinationPortRange: '443'
          destinationAddressPrefix: '*'
          description: 'Primary application traffic.'
        }
      }
      {
        name: 'DenyAllInbound'
        properties: {
          priority: 4096
          access: 'Deny'
          direction: 'Inbound'
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefix: '*'
          destinationPortRange: '*'
          destinationAddressPrefix: '*'
          description: 'Explicit catch-all deny so adding lax rules does not silently expose ports.'
        }
      }
    ]
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2023-02-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: subnetName
        properties: {
          addressPrefix: '10.0.0.0/24'
          networkSecurityGroup: {
            id: nsg.id
          }
        }
      }
    ]
  }
}

resource publicIp 'Microsoft.Network/publicIPAddresses@2023-02-01' = {
  name: publicIpName
  location: location
  properties: {
    publicIPAllocationMethod: 'Dynamic'
  }
  sku: {
    name: 'Basic'
  }
}

resource nic 'Microsoft.Network/networkInterfaces@2023-02-01' = {
  name: nicName
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          subnet: {
            id: vnet.properties.subnets[0].id
          }
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: publicIp.id
          }
        }
      }
    ]
  }
}

resource vm 'Microsoft.Compute/virtualMachines@2023-03-01' = {
  name: vmName
  location: location
  properties: {
    hardwareProfile: {
      vmSize: vmSize
    }
    osProfile: {
      computerName: vmName
      adminUsername: adminUsername
      linuxConfiguration: {
        disablePasswordAuthentication: true
        ssh: {
          publicKeys: [
            {
              path: '/home/${adminUsername}/.ssh/authorized_keys'
              keyData: adminSshPublicKey
            }
          ]
        }
      }
      customData: base64(loadTextContent('cloud-init.sh'))
    }
    storageProfile: {
      imageReference: ubuntuImage
      osDisk: {
        createOption: 'FromImage'
      }
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: nic.id
        }
      ]
    }
  }
}

output vmPublicIp string = publicIp.properties.ipAddress
