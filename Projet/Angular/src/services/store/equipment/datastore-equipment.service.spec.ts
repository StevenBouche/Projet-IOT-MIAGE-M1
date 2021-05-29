import { TestBed } from '@angular/core/testing';

import { DatastoreEquipmentService } from './datastore-equipment.service';

describe('DatastoreEquipmentService', () => {
  let service: DatastoreEquipmentService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DatastoreEquipmentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
