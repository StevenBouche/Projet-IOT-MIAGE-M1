import { TestBed } from '@angular/core/testing';

import { DatastoreEquipmentStreamService } from './datastore-equipment-stream.service';

describe('DatastoreEquipmentStreamService', () => {
  let service: DatastoreEquipmentStreamService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DatastoreEquipmentStreamService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
