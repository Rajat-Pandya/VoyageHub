export const environment = {
  production: false,
  apiUrl: 'http://localhost:5062/api',
  demoPassword: 'VoyageDemo2026!',
  demoPersonas: [
    {
      id: 'A1111111-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
      fullname: 'Rohan Sharma',
      email: 'rohan.sharma@voyagehub.in',
      role: 'Employee',
      departmentName: 'Sales & Business Dev (Mumbai HQ)',
      phoneNumber: '+91 98201 44521',
      homeCity: 'Mumbai (BOM)',
      employeeCode: 'VH-EMP-1042',
      notifyTripStatus: true,
      notifyExpensePayout: true,
      notifyDailyDigest: false,
      notifySmsAlerts: true
    },
    {
      id: 'B2222222-BBBB-BBBB-BBBB-BBBBBBBBBBBB',
      fullname: 'Priya Nair',
      email: 'priya.nair@voyagehub.in',
      role: 'Manager',
      departmentName: 'Sales & Business Dev (India)',
      phoneNumber: '+91 98112 55903',
      homeCity: 'Bengaluru (BLR)',
      employeeCode: 'VH-MGR-2018',
      notifyTripStatus: true,
      notifyExpensePayout: false,
      notifyDailyDigest: true,
      notifySmsAlerts: true
    },
    {
      id: 'C3333333-CCCC-CCCC-CCCC-CCCCCCCCCCCC',
      fullname: 'Vikram Malhotra',
      email: 'vikram.malhotra@voyagehub.in',
      role: 'Finance',
      departmentName: 'Finance & Tax Operations',
      phoneNumber: '+91 99304 88712',
      homeCity: 'New Delhi (DEL)',
      employeeCode: 'VH-FIN-3004',
      notifyTripStatus: false,
      notifyExpensePayout: true,
      notifyDailyDigest: true,
      notifySmsAlerts: false
    },
    {
      id: 'F6666666-FFFF-FFFF-FFFF-FFFFFFFFFFFF',
      fullname: 'Ananya Verma',
      email: 'admin@voyagehub.in',
      role: 'Admin',
      departmentName: 'Corporate Administration',
      phoneNumber: '+91 98450 11988',
      homeCity: 'Hyderabad (HYD)',
      employeeCode: 'VH-ADM-0001',
      notifyTripStatus: true,
      notifyExpensePayout: true,
      notifyDailyDigest: true,
      notifySmsAlerts: true
    }
  ]
};
